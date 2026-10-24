using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class HistorialMedicoController : Controller
    {
        private readonly AsiloDbContext _context;

        public HistorialMedicoController(AsiloDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // VER HISTORIAL MÉDICO
        //
        // Administrador, médicos y enfermero pueden consultar.
        // Si viene desde una visita médica, conserva visitaId
        // para poder regresar a esa misma visita.
        // ============================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista,Enfermero")]
        public async Task<IActionResult> Index(
            int pacienteId,
            int? visitaId)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == pacienteId);

            if (paciente == null)
            {
                return NotFound();
            }

            var historial = await _context.HistorialesMedicos
                .Where(h => h.PacienteId == pacienteId)
                .OrderByDescending(h => h.Fecha)
                .ToListAsync();

            ViewBag.Paciente = paciente;
            ViewBag.PacienteId = pacienteId;
            ViewBag.VisitaId = visitaId;

            return View(historial);
        }


        // ============================================================
        // CREAR REGISTRO DE HISTORIAL
        // Solo administrador y médicos
        // ============================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista")]
        public async Task<IActionResult> Crear(int pacienteId)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == pacienteId);

            if (paciente == null)
            {
                return NotFound();
            }

            ViewBag.Paciente = paciente;

            var historial = new HistorialMedico
            {
                PacienteId = pacienteId,
                Fecha = DateTime.Now
            };

            return View(historial);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista")]
        public async Task<IActionResult> Crear(
            HistorialMedico historial)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p =>
                    p.Id == historial.PacienteId);

            if (paciente == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Paciente = paciente;

                return View(historial);
            }

            historial.Fecha = DateTime.Now;

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Registro agregado al historial médico correctamente.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    pacienteId = historial.PacienteId
                });
        }
    }
}