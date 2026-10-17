using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Farmacia")]
    public class FarmaciaController : Controller
    {
        private readonly AsiloDbContext _context;

        public FarmaciaController(AsiloDbContext context)
        {
            _context = context;
        }

        // =========================================================
        // LISTADO DE RECETAS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recetas = await _context.Recetas
                .Include(r => r.Paciente)
                .Include(r => r.Detalles)

                // Farmacia solo necesita recetas emitidas
                // y recetas que ya fueron entregadas.
                .Where(r =>
                    r.Estado == "Emitida" ||
                    r.Estado == "Entregada")

                .OrderBy(r => r.Estado == "Entregada")
                .ThenByDescending(r => r.FechaReceta)
                .ToListAsync();

            return View(recetas);
        }


        // =========================================================
        // DETALLE DE RECETA
        // =========================================================

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


        // =========================================================
        // ACTUALIZAR CANTIDAD Y PRECIO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarCosto(
            int detalleId,
            int cantidad,
            decimal precioUnitario)
        {
            var detalle = await _context.DetallesReceta
                .Include(d => d.Receta)
                .FirstOrDefaultAsync(d => d.Id == detalleId);

            if (detalle == null)
            {
                return NotFound();
            }

            if (detalle.Receta == null)
            {
                return NotFound();
            }

            // Una receta entregada ya no puede modificarse
            if (detalle.Receta.Estado == "Entregada")
            {
                TempData["Advertencia"] =
                    "La receta ya fue entregada y no puede modificarse.";

                return RedirectToAction(
                    "Detalle",
                    new { id = detalle.RecetaId }
                );
            }

            // Farmacia solo trabaja recetas emitidas
            if (detalle.Receta.Estado != "Emitida")
            {
                TempData["Advertencia"] =
                    "La receta todavía no ha sido emitida por el médico.";

                return RedirectToAction(
                    "Detalle",
                    new { id = detalle.RecetaId }
                );
            }

            if (cantidad <= 0)
            {
                TempData["Advertencia"] =
                    "La cantidad entregada debe ser mayor que cero.";

                return RedirectToAction(
                    "Detalle",
                    new { id = detalle.RecetaId }
                );
            }

            if (precioUnitario <= 0)
            {
                TempData["Advertencia"] =
                    "El precio unitario debe ser mayor que cero.";

                return RedirectToAction(
                    "Detalle",
                    new { id = detalle.RecetaId }
                );
            }

            detalle.Cantidad = cantidad;
            detalle.PrecioUnitario = precioUnitario;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Cantidad y precio guardados correctamente.";

            return RedirectToAction(
                "Detalle",
                new { id = detalle.RecetaId }
            );
        }


        // =========================================================
        // CONFIRMAR ENTREGA
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entregar(int id)
        {
            var receta = await _context.Recetas
                .Include(r => r.Detalles)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (receta == null)
            {
                return NotFound();
            }

            if (receta.Estado == "Entregada")
            {
                TempData["Advertencia"] =
                    "Esta receta ya fue entregada anteriormente.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            if (receta.Estado != "Emitida")
            {
                TempData["Advertencia"] =
                    "La receta todavía no ha sido emitida por el médico.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            if (!receta.Detalles.Any())
            {
                TempData["Advertencia"] =
                    "La receta no contiene medicamentos.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            // Todos los medicamentos deben tener cantidad y precio
            var datosIncompletos = receta.Detalles.Any(d =>
                d.Cantidad <= 0 ||
                d.PrecioUnitario <= 0);

            if (datosIncompletos)
            {
                TempData["Advertencia"] =
                    "Debe guardar la cantidad y el precio de todos los medicamentos antes de confirmar la entrega.";

                return RedirectToAction(
                    "Detalle",
                    new { id = receta.Id }
                );
            }

            receta.Estado = "Entregada";

            var medicamentos = string.Join(
                ", ",
                receta.Detalles.Select(d =>
                    $"{d.Medicamento} {d.Dosis}")
            );

            var historial = new HistorialMedico
            {
                PacienteId = receta.PacienteId,
                Fecha = DateTime.Now,
                TipoRegistro = "Entrega de medicamentos",
                Descripcion =
                    $"Medicamentos entregados: {medicamentos}",
                Observaciones = "",
                ProfesionalResponsable = "Farmacia"
            };

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
     "Los medicamentos fueron entregados correctamente.";

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }
    }
}