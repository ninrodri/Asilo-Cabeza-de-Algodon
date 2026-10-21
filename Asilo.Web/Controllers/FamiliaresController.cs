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

        // =========================================================
        // LISTADO
        // =========================================================

        public async Task<IActionResult> Index()
        {
            var familiares = await _context.Familiares
                .Include(f => f.Paciente)
                .OrderByDescending(f => f.Activo)
                .ThenBy(f => f.NombreCompleto)
                .ToListAsync();

            return View(familiares);
        }


        // =========================================================
        // REGISTRAR
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(Familiar familiar)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(familiar);
            }

            // Todo familiar nuevo inicia activo
            familiar.Activo = true;

            _context.Familiares.Add(familiar);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El familiar fue registrado correctamente.";

            return RedirectToAction("Index");
        }


        // =========================================================
        // EDITAR
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var familiar = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View(familiar);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Familiar familiar)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(familiar);
            }

            var familiarExistente = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == familiar.Id);

            if (familiarExistente == null)
            {
                return NotFound();
            }

            familiarExistente.NombreCompleto =
                familiar.NombreCompleto;

            familiarExistente.Parentesco =
                familiar.Parentesco;

            familiarExistente.Telefono =
                familiar.Telefono;

            familiarExistente.Correo =
                familiar.Correo;

            familiarExistente.Direccion =
                familiar.Direccion;

            familiarExistente.PacienteId =
                familiar.PacienteId;

            // Importante:
            // no modificamos Activo desde Editar.

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Los datos del familiar fueron actualizados.";

            return RedirectToAction("Index");
        }


        // =========================================================
        // DESACTIVAR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desactivar(int id)
        {
            var familiar = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            if (!familiar.Activo)
            {
                TempData["Aviso"] =
                    "El familiar ya se encuentra inactivo.";

                return RedirectToAction("Index");
            }

            familiar.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El familiar fue desactivado. Su historial se conserva.";

            return RedirectToAction("Index");
        }


        // =========================================================
        // REACTIVAR
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var familiar = await _context.Familiares
                .FirstOrDefaultAsync(f => f.Id == id);

            if (familiar == null)
            {
                return NotFound();
            }

            if (familiar.Activo)
            {
                TempData["Aviso"] =
                    "El familiar ya se encuentra activo.";

                return RedirectToAction("Index");
            }

            familiar.Activo = true;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El familiar fue reactivado correctamente.";

            return RedirectToAction("Index");
        }
    }
}