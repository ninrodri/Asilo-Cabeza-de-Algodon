using Asilo.Web.Data;
using Asilo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AsiloDbContext _context;

        public DashboardController(
            AsiloDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index()
        {
            int misVisitasAsignadas = 0;


            // =====================================================
            // VISITAS PROGRAMADAS DEL ENFERMERO ACTUAL
            // =====================================================

            if (User.IsInRole("Enfermero"))
            {
                var idClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (int.TryParse(idClaim, out int enfermeroId))
                {
                    misVisitasAsignadas =
                        await _context.SolicitudesMedicas
                            .CountAsync(s =>
                                s.EnfermeroId == enfermeroId &&
                                s.Estado == "Programada");
                }
            }


            // =====================================================
            // DATOS DEL DASHBOARD
            // =====================================================

            var model = new DashboardViewModel
            {
                TotalInternos =
                    await _context.Pacientes.CountAsync(),

                SolicitudesPendientes =
                    await _context.SolicitudesMedicas
                        .CountAsync(s =>
                            s.Estado ==
                            "Pendiente de Fundación"),

                ExamenesPendientes =
                    await _context.ExamenesLaboratorio
                        .CountAsync(e =>
                            e.Estado != "Completado"),

                CobrosPendientes =
                    await _context.Cobros
                        .CountAsync(c =>
                            c.Estado != "Pagado"),

                MisVisitasAsignadas =
                    misVisitasAsignadas
            };


            return View(model);
        }
    }
}