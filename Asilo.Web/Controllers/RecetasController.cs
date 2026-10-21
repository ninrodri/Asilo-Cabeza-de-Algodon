using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    [Authorize]
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
        [Authorize(Roles = "Administrador,Médico especialista")]
        public async Task<IActionResult> Crear(int visitaId)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .FirstOrDefaultAsync(v => v.Id == visitaId);

            if (visita == null)
            {
                return NotFound();
            }

            // Validar que el especialista actual sea
            // el responsable de la visita.
            if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual = User.Identity?.Name;

                if (visita.Medico != usuarioActual)
                {
                    return Forbid();
                }
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
        [Authorize(Roles = "Administrador,Médico especialista")]
        public async Task<IActionResult> Crear(Receta receta)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .FirstOrDefaultAsync(v =>
                    v.Id == receta.VisitaMedicaId);

            if (visita == null)
            {
                return NotFound();
            }

            if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual = User.Identity?.Name;

                if (visita.Medico != usuarioActual)
                {
                    return Forbid();
                }
            }

            receta.PacienteId = visita.PacienteId;
            receta.Medico = visita.Medico;
            receta.IndicacionesGenerales = "";

            ModelState.Remove(
                nameof(receta.IndicacionesGenerales)
            );

            ModelState.Remove(
                nameof(receta.Medico)
            );

            ModelState.Remove(
                nameof(receta.Paciente)
            );

            if (!ModelState.IsValid)
            {
                receta.Paciente = visita.Paciente;

                return View(receta);
            }

            var recetaExistente = await _context.Recetas
                .FirstOrDefaultAsync(r =>
                    r.VisitaMedicaId ==
                    receta.VisitaMedicaId);

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
        [Authorize(Roles = "Administrador,Médico especialista")]
        public async Task<IActionResult> AgregarMedicamento(
            int recetaId)
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

            if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual = User.Identity?.Name;

                if (receta.Medico != usuarioActual)
                {
                    return Forbid();
                }
            }

            if (receta.Estado == "Emitida" ||
                receta.Estado == "Entregada")
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
                RecetaId = receta.Id,

                // Valores iniciales útiles
                DosisPorToma = 1,
                TomasPorDia = 1,
                DuracionDias = 1,
                ContenidoPresentacion = 1
            };

            return View(detalle);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Médico especialista")]
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

            if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual = User.Identity?.Name;

                if (receta.Medico != usuarioActual)
                {
                    return Forbid();
                }
            }

            if (receta.Estado == "Emitida" ||
                receta.Estado == "Entregada")
            {
                TempData["Aviso"] =
                    "La receta ya fue finalizada y no puede modificarse.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            // Validaciones adicionales
            if (detalle.DosisPorToma <= 0)
            {
                ModelState.AddModelError(
                    nameof(detalle.DosisPorToma),
                    "La cantidad por toma debe ser mayor que cero."
                );
            }

            if (detalle.TomasPorDia <= 0)
            {
                ModelState.AddModelError(
                    nameof(detalle.TomasPorDia),
                    "Las tomas por día deben ser mayores que cero."
                );
            }

            if (detalle.DuracionDias <= 0)
            {
                ModelState.AddModelError(
                    nameof(detalle.DuracionDias),
                    "La duración debe ser mayor que cero."
                );
            }

            if (detalle.ContenidoPresentacion <= 0)
            {
                ModelState.AddModelError(
                    nameof(detalle.ContenidoPresentacion),
                    "El contenido de la presentación debe ser mayor que cero."
                );
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Receta = receta;

                return View(detalle);
            }


            // =====================================================
            // GENERAR LOS TEXTOS LEGIBLES
            // =====================================================

            detalle.Dosis =
                $"{detalle.DosisPorToma:0.##} {detalle.UnidadDosis}";

            detalle.Frecuencia =
                detalle.TomasPorDia == 1
                ? "1 vez al día"
                : $"{detalle.TomasPorDia} veces al día";

            detalle.Duracion =
                detalle.DuracionDias == 1
                ? "1 día"
                : $"{detalle.DuracionDias} días";


            // Farmacia todavía no ha entregado nada.
            detalle.Cantidad = 0;
            detalle.PrecioUnitario = 0;


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
        [Authorize(Roles = "Administrador,Médico especialista")]
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

            if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual = User.Identity?.Name;

                if (receta.Medico != usuarioActual)
                {
                    return Forbid();
                }
            }

            if (receta.Estado == "Emitida" ||
                receta.Estado == "Entregada")
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
        [Authorize(
            Roles =
            "Administrador,Médico general,Médico especialista,Farmacia")]
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
        [Authorize(
            Roles =
            "Administrador,Médico general,Médico especialista")]
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