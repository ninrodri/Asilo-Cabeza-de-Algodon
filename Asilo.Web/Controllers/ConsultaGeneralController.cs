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

            consulta.FechaConsulta = DateTime.Now;

            _context.ConsultasGenerales.Add(consulta);

            var historial = new HistorialMedico
            {
                PacienteId = consulta.PacienteId,
                Fecha = consulta.FechaConsulta,
                TipoRegistro = "Consulta médica general",
                Descripcion =
                    $"Motivo: {consulta.Motivo}. " +
                    $"Diagnóstico: {consulta.Diagnostico}.",
                Observaciones = consulta.Observaciones,
                ProfesionalResponsable = consulta.Medico
            };

            _context.HistorialesMedicos.Add(historial);

            if (consulta.RequiereEspecialista)
            {
                if (string.IsNullOrWhiteSpace(consulta.EspecialidadRequerida))
                {
                    ModelState.AddModelError(
                        nameof(consulta.EspecialidadRequerida),
                        "Debe seleccionar la especialidad requerida."
                    );
                }

                if (string.IsNullOrWhiteSpace(consulta.MotivoReferencia))
                {
                    ModelState.AddModelError(
                        nameof(consulta.MotivoReferencia),
                        "Debe indicar el motivo de la referencia."
                    );
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Pacientes = await _context.Pacientes
                        .OrderBy(p => p.NombreCompleto)
                        .ToListAsync();

                    return View(consulta);
                }

                var solicitud = new SolicitudMedica
                {
                    PacienteId = consulta.PacienteId,
                    FechaSolicitud = DateTime.Now,
                    Motivo = consulta.MotivoReferencia ?? "",
                    Observaciones = consulta.Observaciones,
                    Estado = "Pendiente de Fundación",
                    Especialidad = consulta.EspecialidadRequerida
                };

                _context.SolicitudesMedicas.Add(solicitud);
            }

            await _context.SaveChangesAsync();

            if (consulta.RequiereEspecialista)
            {
                TempData["Exito"] =
                    "La consulta fue guardada y la solicitud médica fue enviada a Fundación.";
            }
            else
            {
                TempData["Exito"] =
                    "La consulta médica fue guardada correctamente.";
            }

            return RedirectToAction(
                "Index",
                "Dashboard"
            );
        }
    }
}