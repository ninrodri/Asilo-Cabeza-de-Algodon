using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class VisitaMedicaController : Controller
    {
        private readonly AsiloDbContext _context;

        public VisitaMedicaController(AsiloDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // CREAR VISITA ESPECIALIZADA
        // Solo especialista asignado o administrador
        // =====================================================

        [HttpGet]
        [Authorize(Roles = "Administrador,Médico especialista")]
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

            if (solicitud.Estado != "Programada")
            {
                TempData["Aviso"] =
                    "La solicitud debe estar programada antes de registrar la visita.";

                return RedirectToAction("Index", "Solicitudes");
            }

            if (!User.IsInRole("Administrador") &&
                solicitud.MedicoEspecialista?.NombreUsuario != User.Identity?.Name)
            {
                return Forbid();
            }

            var visitaExistente = await _context.VisitasMedicas
                .FirstOrDefaultAsync(v =>
                    v.SolicitudMedicaId == solicitudId);

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
                Medico = solicitud.MedicoEspecialista?.NombreUsuario ?? ""
            };

            return View(visita);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Médico especialista")]
        public async Task<IActionResult> Crear(VisitaMedica visita)
        {
            var solicitud = await _context.SolicitudesMedicas
                .Include(s => s.MedicoEspecialista)
                .FirstOrDefaultAsync(s =>
                    s.Id == visita.SolicitudMedicaId);

            if (solicitud == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador") &&
                solicitud.MedicoEspecialista?.NombreUsuario != User.Identity?.Name)
            {
                return Forbid();
            }

            visita.Medico =
                solicitud.MedicoEspecialista?.NombreUsuario ?? "";

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
                TipoRegistro = "Visita médica especializada",
                Descripcion =
                    $"Diagnóstico: {visita.Diagnostico}",
                Observaciones = visita.Observaciones,
                ProfesionalResponsable = visita.Medico
            };

            _context.HistorialesMedicos.Add(historial);

            solicitud.Estado = "Atendida";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La visita médica fue registrada correctamente.";

            return RedirectToAction(
                "Ver",
                new { id = visita.Id }
            );
        }


        // =====================================================
        // VER VISITA
        // Médicos pueden consultar.
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista")]
        public async Task<IActionResult> Ver(int id)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.SolicitudMedica)
                .Include(v => v.ExamenesLaboratorio)
                .Include(v => v.Receta)
                    .ThenInclude(r => r!.Detalles)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (visita == null)
            {
                return NotFound();
            }

            return View(visita);
        }
    }
}