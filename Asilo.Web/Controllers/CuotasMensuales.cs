using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class CuotasMensualesController : Controller
    {
        private readonly AsiloDbContext _context;

        public CuotasMensualesController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cuotas = await _context.CuotasMensuales
                .Include(c => c.Paciente)
                .Include(c => c.Cobro)
                    .ThenInclude(c => c!.Pagos)
                .OrderByDescending(c => c.Anio)
                .ThenByDescending(c => c.Mes)
                .ToListAsync();

            return View(cuotas);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View(new CuotaMensual
            {
                Mes = DateTime.Now.Month,
                Anio = DateTime.Now.Year
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CuotaMensual cuota)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(cuota);
            }

            var pacienteExiste = await _context.Pacientes
                .AnyAsync(p => p.Id == cuota.PacienteId);

            if (!pacienteExiste)
            {
                ModelState.AddModelError(
                    "",
                    "El paciente seleccionado no existe."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(cuota);
            }

            var cuotaExistente = await _context.CuotasMensuales
                .AnyAsync(c =>
                    c.PacienteId == cuota.PacienteId &&
                    c.Mes == cuota.Mes &&
                    c.Anio == cuota.Anio);

            if (cuotaExistente)
            {
                ModelState.AddModelError(
                    "",
                    "Ya existe una cuota mensual para este paciente en el mes y año seleccionados."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(cuota);
            }

            var nombreMes =
                new DateTime(cuota.Anio, cuota.Mes, 1)
                .ToString("MMMM");

            var cobro = new Cobro
            {
                PacienteId = cuota.PacienteId,
                FechaCobro = DateTime.Now,
                Subtotal = cuota.Monto,
                PorcentajeDescuento = 0,
                MontoDescuento = 0,
                Total = cuota.Monto,
                Estado = "Pendiente"
            };

            cobro.Detalles.Add(new DetalleCobro
            {
                Tipo = "Cuota mensual",
                Descripcion =
                    $"Cuota mensual correspondiente a {nombreMes} {cuota.Anio}",
                Monto = cuota.Monto
            });

            _context.Cobros.Add(cobro);

            await _context.SaveChangesAsync();

            cuota.FechaGeneracion = DateTime.Now;
            cuota.Estado = "Pendiente";
            cuota.CobroId = cobro.Id;

            _context.CuotasMensuales.Add(cuota);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La cuota mensual fue generada correctamente.";

            return RedirectToAction("Index");
        }
    }
}