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

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var familiar = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            ViewBag.Pacientes = await _context.Pacientes.ToListAsync();

            return View(familiar);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(Familiar familiar)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes.ToListAsync();
                return View(familiar);
            }

            var familiarExistente = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == familiar.Id);

            if (familiarExistente == null)
            {
                return NotFound();
            }

            familiarExistente.NombreCompleto = familiar.NombreCompleto;
            familiarExistente.Parentesco = familiar.Parentesco;
            familiarExistente.Telefono = familiar.Telefono;
            familiarExistente.Correo = familiar.Correo;
            familiarExistente.Direccion = familiar.Direccion;
            familiarExistente.PacienteId = familiar.PacienteId;

            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Eliminar(int id)
        {
            var familiar = await _context.Familiares
                .Include(f => f.Paciente)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            return View(familiar);
        }

        [HttpPost, ActionName("Eliminar")]
        public async Task<IActionResult> ConfirmarEliminar(int id)
        {
            var familiar = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            _context.Familiares.Remove(familiar);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}