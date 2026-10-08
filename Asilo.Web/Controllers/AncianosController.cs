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

        public async Task<IActionResult> Index(string? buscar)
        {
            var consulta = _context.Pacientes.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                consulta = consulta.Where(p =>
                    EF.Functions.Like(
                        p.NombreCompleto,
                        $"%{buscar}%"
                    )
                );
            }

            ViewBag.Buscar = buscar;

            var pacientes = await consulta
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View(pacientes);
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var paciente = await _context.Pacientes
                .Include(p => p.Familiares)
                .Include(p => p.FichaMedica)
                .Include(p => p.HistorialMedico)
                .Include(p => p.SolicitudesMedicas)
                .Include(p => p.VisitasMedicas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (paciente == null)
            {
                return NotFound();
            }
            return View(paciente);
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