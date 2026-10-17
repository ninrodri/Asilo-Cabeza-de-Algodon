using Asilo.Web.Data;
using Asilo.Web.Models;
using Asilo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class FundacionFinanzasController : Controller
    {
        private readonly AsiloDbContext _context;

        public FundacionFinanzasController(AsiloDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var detalles = await _context.DetallesCobro
                .AsNoTracking()
                .ToListAsync();

            var totalConsultas = detalles
                .Where(d => d.Tipo == "Consulta médica")
                .Sum(d => d.Monto);

            var totalLaboratorio = detalles
                .Where(d => d.Tipo == "Laboratorio")
                .Sum(d => d.Monto);

            var totalMedicamentos = detalles
                .Where(d => d.Tipo == "Medicamento")
                .Sum(d => d.Monto);

            var totalPagado = await _context.PagosFundacion
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var modelo = new CuentaFundacionViewModel
            {
                TotalConsultas = totalConsultas,
                TotalLaboratorio = totalLaboratorio,
                TotalMedicamentos = totalMedicamentos,
                TotalPagado = totalPagado
            };

            return View(modelo);
        }

        [HttpGet]
        public IActionResult RegistrarPago()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(PagoFundacion pago)
        {
            if (pago.Monto <= 0)
            {
                ModelState.AddModelError(
                    nameof(pago.Monto),
                    "El monto debe ser mayor que cero."
                );
            }

            // Calculamos lo que actualmente se debe a Fundación
            var totalAdeudado = await _context.DetallesCobro
                .Where(d =>
                    d.Tipo == "Consulta médica" ||
                    d.Tipo == "Laboratorio" ||
                    d.Tipo == "Medicamento")
                .SumAsync(d => (decimal?)d.Monto) ?? 0;

            var totalPagado = await _context.PagosFundacion
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var saldoPendiente = totalAdeudado - totalPagado;

            if (pago.Monto > saldoPendiente)
            {
                ModelState.AddModelError(
                    nameof(pago.Monto),
                    $"El pago no puede ser mayor al saldo pendiente de Q {saldoPendiente:N2}."
                );
            }

            if (!ModelState.IsValid)
            {
                return View(pago);
            }

            pago.FechaPago = DateTime.Now;

            _context.PagosFundacion.Add(pago);
            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El pago a Fundación fue registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }


    }
}