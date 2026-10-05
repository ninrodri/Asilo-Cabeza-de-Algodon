using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class FamiliaresController : Controller
    {
        private readonly AsiloDbContext _context;

        public FamiliaresController(AsiloDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var familiares = await _context.Familiares
                .Include(f => f.Paciente)
                .ToListAsync();

            return View(familiares);
        }

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            ViewBag.Pacientes = await _context.Pacientes.ToListAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Familiar familiar)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes.ToListAsync();
                return View(familiar);
            }

            _context.Familiares.Add(familiar);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}