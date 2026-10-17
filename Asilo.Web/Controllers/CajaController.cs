using Asilo.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class CajaController : Controller
    {
        private readonly AsiloDbContext _context;

        public CajaController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var totalDonaciones = await _context.Donaciones
        .SumAsync(d => (decimal?)d.Monto) ?? 0;

            var totalPagos = await _context.Pagos
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var totalGastos = await _context.Gastos
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var totalPagosFundacion = await _context.PagosFundacion
                .SumAsync(p => (decimal?)p.Monto) ?? 0;

            var totalEntradas =
                totalDonaciones +
                totalPagos;

            var totalSalidas =
                totalGastos +
                totalPagosFundacion;

            var saldoDisponible =
                totalEntradas -
                totalSalidas;

            ViewBag.TotalDonaciones = totalDonaciones;
            ViewBag.TotalPagos = totalPagos;
            ViewBag.TotalGastos = totalGastos;
            ViewBag.TotalPagosFundacion = totalPagosFundacion;
            ViewBag.TotalEntradas = totalEntradas;
            ViewBag.TotalSalidas = totalSalidas;
            ViewBag.SaldoDisponible = saldoDisponible;

            var donaciones = await _context.Donaciones
                .OrderByDescending(d => d.FechaDonacion)
                .ToListAsync();

            var pagos = await _context.Pagos
                .Include(p => p.Cobro)
                    .ThenInclude(c => c!.Paciente)
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            var gastos = await _context.Gastos
                .OrderByDescending(g => g.FechaGasto)
                .ToListAsync();

            var pagosFundacion = await _context.PagosFundacion
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            ViewBag.Donaciones = donaciones;
            ViewBag.Pagos = pagos;
            ViewBag.Gastos = gastos;
            ViewBag.PagosFundacion = pagosFundacion;

            return View();
        }
    }
}