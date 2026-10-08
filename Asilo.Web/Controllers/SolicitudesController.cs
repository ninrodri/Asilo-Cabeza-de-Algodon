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
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View();
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

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == solicitud.PacienteId);

            if (paciente == null)
            {
                ModelState.AddModelError(
                    "",
                    "El paciente seleccionado no existe."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(solicitud);
            }

            solicitud.FechaSolicitud = DateTime.Now;
            solicitud.Estado = "Pendiente de Fundación";

            solicitud.Especialidad = null;
            solicitud.MedicoEspecialistaId = null;
            solicitud.EnfermeroId = null;
            solicitud.FechaAtencion = null;

            _context.SolicitudesMedicas.Add(solicitud);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La solicitud médica fue enviada correctamente a Fundación.";

            return RedirectToAction("Index");
        }
    }
}