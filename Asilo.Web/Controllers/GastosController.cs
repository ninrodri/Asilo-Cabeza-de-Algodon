using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class GastosController : Controller
    {
        private readonly AsiloDbContext _context;

        public GastosController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var gastos = await _context.Gastos
                .OrderByDescending(g => g.FechaGasto)
                .ToListAsync();

            return View(gastos);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Gasto
            {
                FechaGasto = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Gasto gasto)
        {
            if (!ModelState.IsValid)
            {
                return View(gasto);
            }

            if (gasto.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Monto",
                    "El monto del gasto debe ser mayor que cero."
                );

                return View(gasto);
            }

            gasto.FechaGasto = DateTime.Now;

            _context.Gastos.Add(gasto);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El gasto fue registrado correctamente.";

            return RedirectToAction("Index");
        }
    }
}