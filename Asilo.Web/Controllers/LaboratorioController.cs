using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class LaboratorioController : Controller
    {
        private readonly AsiloDbContext _context;

        public LaboratorioController(AsiloDbContext context)
        {
            _context = context;
        }

        // =====================================================
        // LISTADO
        // Médicos pueden consultar.
        // Laboratorio puede trabajar los exámenes.
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
    "Administrador,Laboratorio,Médico general,Médico especialista")]
        public async Task<IActionResult> Index(int? visitaId)
        {
            var consulta = _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .Include(e => e.VisitaMedica)
                .AsQueryable();

            if (visitaId.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.VisitaMedicaId == visitaId.Value
                );

                ViewBag.VisitaId = visitaId.Value;
            }

            var examenes = await consulta
                .OrderByDescending(e => e.FechaSolicitud)
                .ToListAsync();

            return View(examenes);
        }


        // =====================================================
        // SOLICITAR EXAMEN
        // Solo especialista que realiza la visita.
        // Administrador como respaldo.
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Médico especialista")]
        public async Task<IActionResult> Crear(int visitaId)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .FirstOrDefaultAsync(v => v.Id == visitaId);

            if (visita == null)
            {
                return NotFound();
            }

            // Un especialista no puede solicitar estudios
            // sobre la visita realizada por otro especialista.
            if (!User.IsInRole("Administrador") &&
                visita.Medico != User.Identity?.Name)
            {
                return Forbid();
            }

            var examen = new ExamenLaboratorio
            {
                PacienteId = visita.PacienteId,
                Paciente = visita.Paciente,
                VisitaMedicaId = visita.Id,
                FechaSolicitud = DateTime.Now,
                Estado = "Solicitado"
            };

            return View(examen);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =
            "Administrador,Médico especialista")]
        public async Task<IActionResult> Crear(
            ExamenLaboratorio examen)
        {
            var visita = await _context.VisitasMedicas
                .FirstOrDefaultAsync(v =>
                    v.Id == examen.VisitaMedicaId);

            if (visita == null)
            {
                return NotFound();
            }

            if (!User.IsInRole("Administrador") &&
                visita.Medico != User.Identity?.Name)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                examen.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p =>
                        p.Id == examen.PacienteId);

                return View(examen);
            }

            var examenDuplicado =
                await _context.ExamenesLaboratorio
                    .AnyAsync(e =>
                        e.VisitaMedicaId ==
                            examen.VisitaMedicaId &&
                        e.NombreExamen.ToLower() ==
                            examen.NombreExamen.ToLower());

            if (examenDuplicado)
            {
                ModelState.AddModelError(
                    "NombreExamen",
                    "Este examen ya fue solicitado para esta visita médica."
                );

                examen.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p =>
                        p.Id == examen.PacienteId);

                return View(examen);
            }

            examen.FechaSolicitud = DateTime.Now;
            examen.Estado = "Solicitado";
            examen.Resultado = "";
            examen.FechaResultado = null;
            examen.Costo = 0;

            _context.ExamenesLaboratorio.Add(examen);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El examen fue solicitado correctamente.";

            return RedirectToAction("Index");
        }


        // =====================================================
        // REGISTRAR RESULTADO
        // Solo Laboratorio.
        // Administrador como respaldo.
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Laboratorio")]
        public async Task<IActionResult> Resultado(int id)
        {
            var examen = await _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (examen == null)
            {
                return NotFound();
            }

            return View(examen);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles =
            "Administrador,Laboratorio")]
        public async Task<IActionResult> Resultado(
            ExamenLaboratorio examen)
        {
            var examenExistente =
                await _context.ExamenesLaboratorio
                    .Include(e => e.Paciente)
                    .FirstOrDefaultAsync(e =>
                        e.Id == examen.Id);

            if (examenExistente == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(examen.Resultado))
            {
                ModelState.AddModelError(
                    "Resultado",
                    "El resultado del examen es obligatorio."
                );

                examen.Paciente =
                    examenExistente.Paciente;

                return View(examen);
            }

            if (examen.Costo <= 0)
            {
                ModelState.AddModelError(
                    "Costo",
                    "El costo del examen debe ser mayor que cero."
                );

                examen.Paciente =
                    examenExistente.Paciente;

                return View(examen);
            }

            examenExistente.Resultado =
                examen.Resultado;

            examenExistente.Observaciones =
                examen.Observaciones;

            examenExistente.Costo =
                examen.Costo;

            examenExistente.FechaResultado =
                DateTime.Now;

            examenExistente.Estado =
                "Completado";

            var historial = new HistorialMedico
            {
                PacienteId =
                    examenExistente.PacienteId,

                Fecha =
                    DateTime.Now,

                TipoRegistro =
                    "Resultado de laboratorio",

                Descripcion =
                    $"Examen: {examenExistente.NombreExamen}. " +
                    $"Resultado: {examen.Resultado}",

                Observaciones =
                    examen.Observaciones,

                ProfesionalResponsable =
                    User.Identity?.Name ?? "Laboratorio"
            };

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El resultado fue registrado correctamente.";

            return RedirectToAction("Index");
        }


        // =====================================================
        // VER RESULTADO
        // Médicos y Laboratorio pueden consultarlo.
        // =====================================================

        [HttpGet]
        [Authorize(Roles =
            "Administrador,Laboratorio,Médico general,Médico especialista")]
        public async Task<IActionResult> VerResultado(int id)
        {
            var examen = await _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .Include(e => e.VisitaMedica)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (examen == null)
            {
                return NotFound();
            }

            return View(examen);
        }
    }
}