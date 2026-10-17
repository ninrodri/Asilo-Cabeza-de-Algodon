using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general,Médico especialista")]
    public class VisitaMedicaController : Controller
    {
        private readonly AsiloDbContext _context;

        public VisitaMedicaController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int solicitudId)
        {
            var solicitud = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null)
            {
                return NotFound();
            }

            var visitaExistente = await _context.VisitasMedicas
                .AnyAsync(v => v.SolicitudMedicaId == solicitudId);

            if (visitaExistente)
            {
                TempData["Aviso"] =
                    "Esta solicitud ya tiene una visita médica registrada.";

                return RedirectToAction("Index", "Solicitudes");
            }

            string medico = "";

            if (solicitud.MedicoEspecialistaId.HasValue)
            {
                medico = await _context.Usuarios
                    .Where(u => u.Id == solicitud.MedicoEspecialistaId.Value)
                    .Select(u => u.NombreUsuario)
                    .FirstOrDefaultAsync() ?? "";
            }

            var visita = new VisitaMedica
            {
                SolicitudMedicaId = solicitud.Id,
                PacienteId = solicitud.PacienteId,
                Paciente = solicitud.Paciente,
                Medico = medico
            };

            return View(visita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(VisitaMedica visita)
        {
            var solicitud = await _context.SolicitudesMedicas
                .FirstOrDefaultAsync(s =>
                    s.Id == visita.SolicitudMedicaId);

            if (solicitud == null)
            {
                return NotFound();
            }

            // El médico se obtiene desde la solicitud,
            // no desde lo que mande el navegador.
            if (solicitud.MedicoEspecialistaId.HasValue)
            {
                visita.Medico = await _context.Usuarios
                    .Where(u => u.Id == solicitud.MedicoEspecialistaId.Value)
                    .Select(u => u.NombreUsuario)
                    .FirstOrDefaultAsync() ?? "";
            }

            // Quitamos la validación anterior de Medico
            // porque acabamos de asignarlo desde la BD.
            ModelState.Remove(nameof(visita.Medico));

            if (string.IsNullOrWhiteSpace(visita.Medico))
            {
                ModelState.AddModelError(
                    "",
                    "La solicitud no tiene un médico especialista asignado."
                );
            }

            if (!ModelState.IsValid)
            {
                visita.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p =>
                        p.Id == visita.PacienteId);

                return View(visita);
            }

            var visitaExistente = await _context.VisitasMedicas
                .FirstOrDefaultAsync(v =>
                    v.SolicitudMedicaId == visita.SolicitudMedicaId);

            if (visitaExistente != null)
            {
                return RedirectToAction(
                    "Ver",
                    new { id = visitaExistente.Id }
                );
            }

            visita.FechaVisita = DateTime.Now;

            _context.VisitasMedicas.Add(visita);

            var historial = new HistorialMedico
            {
                PacienteId = visita.PacienteId,
                Fecha = DateTime.Now,
                TipoRegistro = "Visita médica",

                Descripcion =
                    $"Diagnóstico: {visita.Diagnostico}",

                Observaciones =
                    $"Observaciones: {visita.Observaciones}",

                ProfesionalResponsable = visita.Medico
            };

            _context.HistorialesMedicos.Add(historial);

            solicitud.Estado = "Atendida";

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Ver",
                new { id = visita.Id }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Ver(int id)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.SolicitudMedica)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
            {
                return NotFound();
            }

            return View(visita);
        }
    }
}