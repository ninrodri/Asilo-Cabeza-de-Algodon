using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general,Médico especialista")]
    public class RecetasController : Controller
    {
        private readonly AsiloDbContext _context;

        public RecetasController(AsiloDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // CREAR RECETA
        // =========================================================

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
                .FirstOrDefaultAsync(r =>
                    r.VisitaMedicaId == visitaId);

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
                Estado = "Pendiente",
                IndicacionesGenerales = ""
            };

            return View(receta);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Receta receta)
        {
            // Ya no utilizamos IndicacionesGenerales
            receta.IndicacionesGenerales = "";

            ModelState.Remove(
                nameof(receta.IndicacionesGenerales)
            );

            if (!ModelState.IsValid)
            {
                receta.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p =>
                        p.Id == receta.PacienteId);

                return View(receta);
            }

            var recetaExistente = await _context.Recetas
                .FirstOrDefaultAsync(r =>
                    r.VisitaMedicaId == receta.VisitaMedicaId);

            if (recetaExistente != null)
            {
                return RedirectToAction(
                    "Detalle",
                    new { id = recetaExistente.Id }
                );
            }

            receta.FechaReceta = DateTime.Now;
            receta.Estado = "Pendiente";

            _context.Recetas.Add(receta);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Receta creada correctamente. Ahora puede agregar los medicamentos.";

            return RedirectToAction(
                "AgregarMedicamento",
                new { recetaId = receta.Id }
            );
        }


        // =========================================================
        // AGREGAR MEDICAMENTO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> AgregarMedicamento(int recetaId)
        {
            var receta = await _context.Recetas
                .Include(r => r.Paciente)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r =>
                    r.Id == recetaId);

            if (receta == null)
            {
                return NotFound();
            }

            // Una receta finalizada ya no se modifica
            if (receta.Estado == "Emitida")
            {
                TempData["Aviso"] =
                    "La receta ya fue finalizada y no puede modificarse.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
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
            var receta = await _context.Recetas
                .Include(r => r.Paciente)
                .FirstOrDefaultAsync(r =>
                    r.Id == detalle.RecetaId);

            if (receta == null)
            {
                return NotFound();
            }

            if (receta.Estado == "Emitida")
            {
                TempData["Aviso"] =
                    "La receta ya fue finalizada y no puede modificarse.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Receta = receta;

                return View(detalle);
            }

            _context.DetallesReceta.Add(detalle);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Medicamento agregado correctamente a la receta.";

            return RedirectToAction(
                "Detalle",
                new { id = detalle.RecetaId }
            );
        }


        // =========================================================
        // FINALIZAR RECETA
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            var receta = await _context.Recetas
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r =>
                    r.Id == id);

            if (receta == null)
            {
                return NotFound();
            }

            if (receta.Estado == "Emitida")
            {
                TempData["Aviso"] =
                    "Esta receta ya fue finalizada.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            if (!receta.Detalles.Any())
            {
                TempData["Advertencia"] =
                    "Debe agregar al menos un medicamento antes de finalizar la receta.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            receta.Estado = "Emitida";

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La receta médica fue finalizada correctamente.";

            return RedirectToAction(
                "Detalle",
                new { id = receta.Id }
            );
        }


        // =========================================================
        // VER RECETA
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var receta = await _context.Recetas
                .Include(r => r.Paciente)
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r =>
                    r.Id == id);

            if (receta == null)
            {
                return NotFound();
            }

            return View(receta);
        }


        // =========================================================
        // REPORTE
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> MedicamentosPorPaciente(
            int? pacienteId,
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;
            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

            var consulta = _context.DetallesReceta
                .Include(d => d.Receta)
                    .ThenInclude(r => r!.Paciente)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(d =>
                    d.Receta != null &&
                    d.Receta.PacienteId ==
                    pacienteId.Value);
            }

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(d =>
                    d.Receta != null &&
                    d.Receta.FechaReceta.Date >=
                    fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(d =>
                    d.Receta != null &&
                    d.Receta.FechaReceta.Date <=
                    fechaFin.Value.Date);
            }

            var medicamentos = await consulta
                .OrderByDescending(d =>
                    d.Receta!.FechaReceta)
                .ToListAsync();

            return View(medicamentos);
        }
    }
}