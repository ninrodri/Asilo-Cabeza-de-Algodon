using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace Asilo.Web.Controllers
{

    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class AncianosController : Controller

    {
        private readonly AsiloDbContext _context;

        public AncianosController(AsiloDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            var pacientes = await _context.Pacientes.ToListAsync();

            return View(pacientes);
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Anciano anciano)
        {
            if (!ModelState.IsValid)
            {
                return View(anciano);
            }

            _context.Pacientes.Update(anciano);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        [HttpPost, ActionName("Eliminar")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var paciente = await _context.Pacientes.FindAsync(id);

            if (paciente == null)
            {
                return NotFound();
            }

            _context.Pacientes.Remove(paciente);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Anciano anciano)
        {
            if (!ModelState.IsValid)
            {
                return View(anciano);
            }

            _context.Pacientes.Add(anciano);
            await _context.SaveChangesAsync();

            return View("Confirmacion", anciano);
        }
    }
}