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

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var recetas = await _context.Recetas
                .Include(r => r.Paciente)
                .Include(r => r.Detalles)
                .OrderBy(r => r.Estado == "Entregada")
                .ThenByDescending(r => r.FechaReceta)
                .ToListAsync();

            return View(recetas);
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

            if (!receta.Detalles.Any())
            {
                TempData["Advertencia"] =
                    "La receta no puede marcarse como entregada porque no tiene medicamentos.";

                return RedirectToAction("Index");
            }

            if (receta.Estado == "Entregada")
            {
                TempData["Advertencia"] =
                    "Esta receta ya fue entregada anteriormente.";

                return RedirectToAction("Index");
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
                Descripcion = $"Medicamentos entregados: {medicamentos}",
                Observaciones = receta.IndicacionesGenerales,
                ProfesionalResponsable = "Farmacia"
            };

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Los medicamentos fueron entregados correctamente.";

            return RedirectToAction("Index");
        }
    }
}