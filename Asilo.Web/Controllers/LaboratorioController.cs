using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Laboratorio,Médico general,Médico especialista")]
    public class LaboratorioController : Controller
    {
        private readonly AsiloDbContext _context;

        public LaboratorioController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var examenes = await _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .Include(e => e.VisitaMedica)
                .OrderByDescending(e => e.FechaSolicitud)
                .ToListAsync();

            return View(examenes);
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int visitaId)
        {
            var visita = await _context.VisitasMedicas
                .Include(v => v.Paciente)
                .FirstOrDefaultAsync(v => v.Id == visitaId);

            if (visita == null)
            {
                return NotFound();
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
        public async Task<IActionResult> Crear(ExamenLaboratorio examen)
        {
            if (!ModelState.IsValid)
            {
                examen.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == examen.PacienteId);

                return View(examen);
            }

            examen.FechaSolicitud = DateTime.Now;
            examen.Estado = "Solicitado";

            var examenDuplicado = await _context.ExamenesLaboratorio
            .AnyAsync(e =>
             e.VisitaMedicaId == examen.VisitaMedicaId &&
             e.NombreExamen.ToLower() == examen.NombreExamen.ToLower());

            if (examenDuplicado)
            {
                ModelState.AddModelError(
                    "NombreExamen",
                    "Este examen ya fue solicitado para esta visita médica."
                );

                examen.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == examen.PacienteId);

                return View(examen);
            }

            _context.ExamenesLaboratorio.Add(examen);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        [HttpGet]
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
        public async Task<IActionResult> Resultado(ExamenLaboratorio examen)
        {
            var examenExistente = await _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .FirstOrDefaultAsync(e => e.Id == examen.Id);

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

                examen.Paciente = examenExistente.Paciente;

                return View(examen);
            }

            examenExistente.Resultado = examen.Resultado;
            examenExistente.Observaciones = examen.Observaciones;
            examenExistente.FechaResultado = DateTime.Now;
            examenExistente.Estado = "Completado";

            var historial = new HistorialMedico
            {
                PacienteId = examenExistente.PacienteId,
                Fecha = DateTime.Now,
                TipoRegistro = "Resultado de laboratorio",
                Descripcion =
                    $"Examen: {examenExistente.NombreExamen}. " +
                    $"Resultado: {examen.Resultado}",
                Observaciones = examen.Observaciones,
                ProfesionalResponsable = "Laboratorio"
            };

            _context.HistorialesMedicos.Add(historial);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El resultado fue registrado correctamente.";

            return RedirectToAction("Index");
        }

        [HttpGet]
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