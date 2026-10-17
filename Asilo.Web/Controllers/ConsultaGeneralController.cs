using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general")]
    public class ConsultaGeneralController : Controller
    {
        private readonly AsiloDbContext _context;

        public ConsultaGeneralController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            return View(new ConsultaGeneral
            {
                FechaConsulta = DateTime.Now,
                Medico = User.Identity?.Name ?? ""
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ConsultaGeneral consulta)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(consulta);
            }

            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == consulta.PacienteId);

            if (paciente == null)
            {
                ModelState.AddModelError(
                    "",
                    "El paciente seleccionado no existe."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(consulta);
            }

            consulta.FechaConsulta = DateTime.Now;
            consulta.Medico = User.Identity?.Name ?? "Médico general";

            _context.ConsultasGenerales.Add(consulta);

            var historial = new HistorialMedico
            {
                PacienteId = consulta.PacienteId,
                Fecha = DateTime.Now,
                TipoRegistro = "Consulta médica general",
                Descripcion =
            $"Motivo: {consulta.Motivo}\n" +
            $"Diagnóstico: {consulta.Diagnostico}",
                Observaciones = consulta.Observaciones,
                ProfesionalResponsable = consulta.Medico
            };

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            if (!consulta.RequiereEspecialista)
            {
                TempData["Exito"] =
                    "La consulta médica fue registrada correctamente.";

                return RedirectToAction(
                    "Index",
                    "Dashboard"
                );
            }

            return RedirectToAction(
                "Crear",
                "Solicitudes",
                new
                {
                    pacienteId = consulta.PacienteId,
                    especialidad = consulta.EspecialidadRequerida,
                    motivo = consulta.MotivoReferencia
                }
            );
        }
    }
}