using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class MedicamentosAplicadosController : Controller
    {
        private readonly AsiloDbContext _context;

        public MedicamentosAplicadosController(AsiloDbContext context)
        {
            _context = context;
        }


        // ============================================================
        // LISTADO DE VISITAS ASIGNADAS AL ENFERMERO
        // ============================================================

        [Authorize(Roles = "Enfermero")]
        [HttpGet]
        public async Task<IActionResult> Index(int? pacienteId)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int enfermeroId))
            {
                return Forbid();
            }


            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;


            var consulta = _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.SolicitudMedica)
                .Where(v =>
                    v.SolicitudMedica != null &&
                    v.SolicitudMedica.EnfermeroId == enfermeroId)
                .AsQueryable();


            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(v =>
                    v.PacienteId == pacienteId.Value);
            }


            var visitas = await consulta
                .OrderByDescending(v => v.FechaVisita)
                .ToListAsync();


            return View(visitas);
        }



        // ============================================================
        // REGISTRAR MEDICAMENTO / TRATAMIENTO APLICADO
        // GET
        // ============================================================

        [Authorize(Roles = "Enfermero")]
        [HttpGet]
        public async Task<IActionResult> Aplicar(int id)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int enfermeroId))
            {
                return Forbid();
            }


            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.SolicitudMedica)
                .FirstOrDefaultAsync(v => v.Id == id);


            if (visita == null)
            {
                return NotFound();
            }


            // El enfermero solamente puede trabajar
            // con visitas donde fue asignado.
            if (visita.SolicitudMedica == null ||
                visita.SolicitudMedica.EnfermeroId != enfermeroId)
            {
                return Forbid();
            }


            ViewBag.Visita = visita;


            var modelo = new MedicamentoAplicado
            {
                VisitaMedicaId = visita.Id,
                FechaAplicacion = DateTime.Now
            };


            return View(modelo);
        }



        // ============================================================
        // REGISTRAR MEDICAMENTO / TRATAMIENTO APLICADO
        // POST
        // ============================================================

        [Authorize(Roles = "Enfermero")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aplicar(
            MedicamentoAplicado modelo)
        {
            var idClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(idClaim, out int enfermeroId))
            {
                return Forbid();
            }


            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .Include(v => v.SolicitudMedica)
                .FirstOrDefaultAsync(v =>
                    v.Id == modelo.VisitaMedicaId);


            if (visita == null)
            {
                return NotFound();
            }


            // Seguridad:
            // solamente puede registrar en sus visitas.
            if (visita.SolicitudMedica == null ||
                visita.SolicitudMedica.EnfermeroId != enfermeroId)
            {
                return Forbid();
            }


            // SQL Server genera el Id.
            modelo.Id = 0;

            // El usuario que registra es el enfermero conectado.
            modelo.RegistradoPorId = enfermeroId;


            ModelState.Remove(
                nameof(modelo.VisitaMedica));

            ModelState.Remove(
                nameof(modelo.RegistradoPor));


            if (!ModelState.IsValid)
            {
                ViewBag.Visita = visita;

                return View(modelo);
            }


            // ========================================================
            // EVITAR DUPLICADOS EXACTOS
            // ========================================================

            var yaExiste = await _context.MedicamentosAplicados
                .AnyAsync(m =>
                    m.VisitaMedicaId ==
                        modelo.VisitaMedicaId &&

                    m.Medicamento ==
                        modelo.Medicamento &&

                    m.CantidadAplicada ==
                        modelo.CantidadAplicada &&

                    m.TiempoAplicacion ==
                        modelo.TiempoAplicacion &&

                    m.FechaAplicacion ==
                        modelo.FechaAplicacion);


            if (yaExiste)
            {
                TempData["Error"] =
                    "Esta aplicación ya fue registrada anteriormente.";


                return RedirectToAction(
                    nameof(Historial),
                    new
                    {
                        pacienteId = visita.PacienteId,
                        visitaId = visita.Id
                    });
            }


            // ========================================================
            // GUARDAR
            // ========================================================

            _context.MedicamentosAplicados.Add(modelo);

            await _context.SaveChangesAsync();


            TempData["Success"] =
                "Medicamento o tratamiento aplicado registrado correctamente.";


            return RedirectToAction(
                nameof(Historial),
                new
                {
                    pacienteId = visita.PacienteId,
                    visitaId = visita.Id
                });
        }



        // ============================================================
        // HISTORIAL DE MEDICAMENTOS APLICADOS
        // ============================================================

        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista,Enfermero")]
        [HttpGet]
        public async Task<IActionResult> Historial(
            int? pacienteId,
            int? visitaId)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;
            ViewBag.VisitaId = visitaId;


            // Si no se seleccionó un paciente,
            // no mostramos todos los historiales automáticamente.
            if (!pacienteId.HasValue)
            {
                ViewBag.SeleccionarPaciente = true;

                return View(
                    new List<MedicamentoAplicado>());
            }


            ViewBag.SeleccionarPaciente = false;


            var consulta = _context.MedicamentosAplicados
                .Include(m => m.VisitaMedica)
                    .ThenInclude(v => v!.Paciente)

                .Include(m => m.VisitaMedica)
                    .ThenInclude(v => v!.SolicitudMedica)

                .Include(m => m.RegistradoPor)

                .Where(m =>
                    m.VisitaMedica != null &&
                    m.VisitaMedica.PacienteId ==
                        pacienteId.Value)

                .AsQueryable();


            // ========================================================
            // ENFERMERO
            // Solo ve registros de visitas donde fue asignado.
            // ========================================================

            if (User.IsInRole("Enfermero"))
            {
                var idClaim = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (!int.TryParse(
                    idClaim,
                    out int enfermeroId))
                {
                    return Forbid();
                }


                consulta = consulta.Where(m =>
                    m.VisitaMedica != null &&
                    m.VisitaMedica.SolicitudMedica != null &&
                    m.VisitaMedica.SolicitudMedica.EnfermeroId ==
                        enfermeroId);
            }


            // ========================================================
            // MÉDICO ESPECIALISTA
            // Solo ve aplicaciones relacionadas con sus visitas.
            // ========================================================

            else if (User.IsInRole("Médico especialista"))
            {
                var usuarioActual =
                    User.Identity?.Name;


                consulta = consulta.Where(m =>
                    m.VisitaMedica != null &&
                    m.VisitaMedica.Medico ==
                        usuarioActual);
            }


            var registros = await consulta
                .OrderByDescending(
                    m => m.FechaAplicacion)
                .ToListAsync();


            return View(registros);
        }
    }
}