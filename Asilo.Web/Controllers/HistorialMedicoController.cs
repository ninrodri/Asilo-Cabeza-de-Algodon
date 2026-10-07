using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general,Médico especialista")]
    public class HistorialMedicoController : Controller
    {
        private readonly AsiloDbContext _context;

        public HistorialMedicoController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int pacienteId)
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

            return View(historial);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int pacienteId)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == pacienteId);

            if (paciente == null)
            {
                return NotFound();
            }

            var historial = new HistorialMedico
            {
                PacienteId = paciente.Id,
                Paciente = paciente,
                Fecha = DateTime.Now
            };

            return View(historial);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(HistorialMedico historial)
        {
            if (!ModelState.IsValid)
            {
                historial.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == historial.PacienteId);

                return View(historial);
            }

            historial.Fecha = DateTime.Now;

            _context.HistorialesMedicos.Add(historial);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Index",
                new { pacienteId = historial.PacienteId }
            );
        }
    }
}