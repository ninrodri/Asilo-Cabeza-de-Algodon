using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asilo.Web.Models;
using Asilo.Web.Data;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general")]
    public class SolicitudesController : Controller
    {
        private readonly AsiloDbContext _context;

        public SolicitudesController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .Include(s => s.MedicoEspecialista)
                .Include(s => s.Enfermero)
                .Include(s => s.VisitaMedica)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(
    int? pacienteId,
    string? especialidad,
    string? motivo)
        {
            if (!pacienteId.HasValue ||
                string.IsNullOrWhiteSpace(especialidad) ||
                string.IsNullOrWhiteSpace(motivo))
            {
                TempData["Aviso"] =
                    "La solicitud médica debe generarse desde una consulta del médico general.";

                return RedirectToAction(
                    "Crear",
                    "ConsultaGeneral"
                );
            }

            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            var solicitud = new SolicitudMedica
            {
                PacienteId = pacienteId.Value,
                Especialidad = especialidad,
                Motivo = motivo
            };

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(SolicitudMedica solicitud)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(solicitud);
            }

            solicitud.FechaSolicitud = DateTime.Now;
            solicitud.Estado = "Pendiente de Fundación";

            _context.SolicitudesMedicas.Add(solicitud);
            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La solicitud médica fue enviada correctamente a Fundación.";

            return RedirectToAction("Index");
        }
    }
}