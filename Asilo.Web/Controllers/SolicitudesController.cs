using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asilo.Web.Models;
using Asilo.Web.Data;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class SolicitudesController : Controller
    {
        private readonly AsiloDbContext _context;

        public SolicitudesController(
            AsiloDbContext context)
        {
            _context = context;
        }


        // =====================================================
        // LISTADO DE SOLICITUDES
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista,Enfermero")]
        public async Task<IActionResult> Index()
        {
            var consulta =
                _context.SolicitudesMedicas

                    .Include(s =>
                        s.Paciente)

                    .Include(s =>
                        s.MedicoEspecialista)

                    .Include(s =>
                        s.Enfermero)

                    .Include(s =>
                        s.VisitaMedica)

                    .AsQueryable();


            // =====================================================
            // MÉDICO ESPECIALISTA
            // Solo ve solicitudes asignadas a él
            // =====================================================

            if (User.IsInRole(
                "Médico especialista"))
            {
                var idUsuarioTexto =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );

                if (!int.TryParse(
                    idUsuarioTexto,
                    out int usuarioId))
                {
                    return Forbid();
                }


                consulta =
                    consulta.Where(s =>
                        s.MedicoEspecialistaId ==
                        usuarioId
                    );
            }


            // =====================================================
            // ENFERMERO
            // Solo ve solicitudes donde fue asignado
            // =====================================================

            else if (User.IsInRole(
                "Enfermero"))
            {
                var idUsuarioTexto =
                    User.FindFirstValue(
                        ClaimTypes.NameIdentifier
                    );

                if (!int.TryParse(
                    idUsuarioTexto,
                    out int usuarioId))
                {
                    return Forbid();
                }


                consulta =
                    consulta.Where(s =>
                        s.EnfermeroId ==
                        usuarioId
                    );
            }


            var solicitudes =
                await consulta

                    .OrderByDescending(s =>
                        s.FechaSolicitud)

                    .ToListAsync();


            return View(solicitudes);
        }


        // =====================================================
        // CREAR SOLICITUD
        // Solo médico general / administrador
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico general")]
        public async Task<IActionResult> Crear(
            int? pacienteId,
            string? especialidad,
            string? motivo)
        {
            if (!pacienteId.HasValue ||
                string.IsNullOrWhiteSpace(
                    especialidad) ||
                string.IsNullOrWhiteSpace(
                    motivo))
            {
                TempData["Aviso"] =
                    "La solicitud médica debe generarse desde una consulta del médico general.";

                return RedirectToAction(
                    "Crear",
                    "ConsultaGeneral"
                );
            }


            ViewBag.Pacientes =
                await _context.Pacientes

                    .OrderBy(p =>
                        p.NombreCompleto)

                    .ToListAsync();


            var solicitud =
                new SolicitudMedica
                {
                    PacienteId =
                        pacienteId.Value,

                    Especialidad =
                        especialidad,

                    Motivo =
                        motivo
                };


            return View(solicitud);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =
            "Administrador,Médico general")]
        public async Task<IActionResult> Crear(
            SolicitudMedica solicitud)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes =
                    await _context.Pacientes

                        .OrderBy(p =>
                            p.NombreCompleto)

                        .ToListAsync();


                return View(solicitud);
            }


            solicitud.FechaSolicitud =
                DateTime.Now;


            solicitud.Estado =
                "Pendiente de Fundación";


            _context.SolicitudesMedicas.Add(
                solicitud
            );


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "La solicitud médica fue enviada correctamente a Fundación.";


            return RedirectToAction(
                "Index"
            );
        }
    }
}