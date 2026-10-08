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
             .Include(s => s.MedicoEspecialista)
             .FirstOrDefaultAsync(s => s.Id == solicitudId);

            if (solicitud == null)
            {
                return NotFound();
            }

            var visitaExistente = await _context.VisitasMedicas
                .FirstOrDefaultAsync(v => v.SolicitudMedicaId == solicitudId);

            if (visitaExistente != null)
            {
                return RedirectToAction(
                    "Ver",
                    new { id = visitaExistente.Id }
                );
            }

            var visita = new VisitaMedica
            {
                SolicitudMedicaId = solicitud.Id,
                PacienteId = solicitud.PacienteId,
                Paciente = solicitud.Paciente,
                Medico = solicitud.MedicoEspecialista?.NombreUsuario ?? "",
                FechaVisita = DateTime.Now
            };

            return View(visita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(VisitaMedica visita)
        {
            if (!ModelState.IsValid)
            {
                visita.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == visita.PacienteId);

                return View(visita);
            }

            var solicitud = await _context.SolicitudesMedicas
                .FirstOrDefaultAsync(s => s.Id == visita.SolicitudMedicaId);

            if (solicitud == null)
            {
                return NotFound();
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
                Descripcion = $"Diagnóstico: {visita.Diagnostico}",
                Observaciones =
                    $"Tratamiento: {visita.Tratamiento}\n" +
                    $"Indicaciones: {visita.Indicaciones}\n" +
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