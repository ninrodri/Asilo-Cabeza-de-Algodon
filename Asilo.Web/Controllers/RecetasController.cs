using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general,Médico especialista,Farmacia")]
    public class RecetasController : Controller
    {
        private readonly AsiloDbContext _context;

        public RecetasController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int visitaId)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .FirstOrDefaultAsync(v => v.Id == visitaId);

            if (visita == null)
            {
                return NotFound();
            }

            var recetaExistente = await _context.Recetas
                .FirstOrDefaultAsync(r => r.VisitaMedicaId == visitaId);

            if (recetaExistente != null)
            {
                return RedirectToAction(
                    "Detalle",
                    new { id = recetaExistente.Id }
                );
            }

            var receta = new Receta
            {
                PacienteId = visita.PacienteId,
                Paciente = visita.Paciente,
                VisitaMedicaId = visita.Id,
                Medico = visita.Medico,
                FechaReceta = DateTime.Now,
                Estado = "Pendiente"
            };

            return View(receta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Receta receta)
        {
            if (!ModelState.IsValid)
            {
                receta.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == receta.PacienteId);

                return View(receta);
            }

            var yaExiste = await _context.Recetas
                .AnyAsync(r => r.VisitaMedicaId == receta.VisitaMedicaId);

            if (yaExiste)
            {
                var existente = await _context.Recetas
                    .FirstAsync(r =>
                        r.VisitaMedicaId == receta.VisitaMedicaId);

                return RedirectToAction(
                    "Detalle",
                    new { id = existente.Id }
                );
            }

            receta.FechaReceta = DateTime.Now;
            receta.Estado = "Pendiente";

            _context.Recetas.Add(receta);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "AgregarMedicamento",
                new { recetaId = receta.Id }
            );
        }

        [HttpGet]
        public async Task<IActionResult> AgregarMedicamento(int recetaId)
        {
            var receta = await _context.Recetas
                .Include(r => r.Paciente)
                .FirstOrDefaultAsync(r => r.Id == recetaId);

            if (receta == null)
            {
                return NotFound();
            }

            ViewBag.Receta = receta;

            var detalle = new DetalleReceta
            {
                RecetaId = receta.Id
            };

            return View(detalle);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarMedicamento(
            DetalleReceta detalle)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Receta = await _context.Recetas
                    .Include(r => r.Paciente)
                    .FirstOrDefaultAsync(r => r.Id == detalle.RecetaId);

                return View(detalle);
            }

            _context.DetallesReceta.Add(detalle);
            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Detalle",
                new { id = detalle.RecetaId }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var receta = await _context.Recetas
                .Include(r => r.Paciente)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }
    }
}