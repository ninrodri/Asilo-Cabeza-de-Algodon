using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class DonacionesController : Controller
    {
        private readonly AsiloDbContext _context;

        public DonacionesController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var donaciones = await _context.Donaciones
                .OrderByDescending(d => d.FechaDonacion)
                .ToListAsync();

            return View(donaciones);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View(new Donacion
            {
                FechaDonacion = DateTime.Now
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Donacion donacion)
        {
            if (!ModelState.IsValid)
            {
                return View(donacion);
            }

            if (donacion.Monto <= 0)
            {
                ModelState.AddModelError(
                    "Monto",
                    "El monto de la donación debe ser mayor que cero."
                );

                return View(donacion);
            }

            donacion.FechaDonacion = DateTime.Now;

            _context.Donaciones.Add(donacion);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La donación fue registrada correctamente.";

            return RedirectToAction("Index");
        }
    }
}