using Asilo.Web.Data;
using Asilo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AsiloDbContext _context;

        public DashboardController(AsiloDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new DashboardViewModel
            {
                TotalInternos = _context.Pacientes.Count(),

                SolicitudesPendientes = _context.SolicitudesMedicas
                    .Count(s => s.Estado == "Pendiente de Fundación"),

                ExamenesPendientes = _context.ExamenesLaboratorio
                    .Count(e => e.Estado != "Completado"),

                CobrosPendientes = _context.Cobros
                    .Count(c => c.Estado != "Pagado")
            };

            return View(model);
        }
    }
}