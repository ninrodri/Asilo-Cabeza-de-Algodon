using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Médico general,Médico especialista")]
    public class FichaMedicaController : Controller
    {
        private readonly AsiloDbContext _context;

        public FichaMedicaController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Ver(int pacienteId)
        {
            var ficha = await _context.FichasMedicas
                .Include(f => f.Paciente)
                .FirstOrDefaultAsync(f => f.PacienteId == pacienteId);

            if (ficha == null)
            {
                return RedirectToAction("Crear", new { pacienteId });
            }

            return View(ficha);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var ficha = await _context.FichasMedicas
                .Include(f => f.Paciente)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (ficha == null)
            {
                return NotFound();
            }

            return View(ficha);
        }

        [HttpPost]
        public async Task<IActionResult> Editar(FichaMedica ficha)
        {
            if (!ModelState.IsValid)
            {
                ficha.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == ficha.PacienteId);

                return View(ficha);
            }

            var fichaExistente = await _context.FichasMedicas
                .FirstOrDefaultAsync(f => f.Id == ficha.Id);

            if (fichaExistente == null)
            {
                return NotFound();
            }

            fichaExistente.Padecimientos = ficha.Padecimientos;
            fichaExistente.Enfermedades = ficha.Enfermedades;
            fichaExistente.Psicopatologias = ficha.Psicopatologias;
            fichaExistente.MedicamentosFrecuentes = ficha.MedicamentosFrecuentes;
            fichaExistente.Alergias = ficha.Alergias;
            fichaExistente.ObservacionesMedicas = ficha.ObservacionesMedicas;
            fichaExistente.FechaActualizacion = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(
                "Ver",
                new { pacienteId = fichaExistente.PacienteId }
            );
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int pacienteId)
        {
            var paciente = await _context.Pacientes
                .FirstOrDefaultAsync(p => p.Id == pacienteId);

            if (paciente == null)
            {
                return NotFound();
            }

            var ficha = new FichaMedica
            {
                PacienteId = paciente.Id,
                Paciente = paciente
            };

            return View(ficha);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(FichaMedica ficha)
        {
            if (!ModelState.IsValid)
            {
                ficha.Paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.Id == ficha.PacienteId);

                return View(ficha);
            }

            var yaExiste = await _context.FichasMedicas
                .AnyAsync(f => f.PacienteId == ficha.PacienteId);

            if (yaExiste)
            {
                return RedirectToAction("Ver", new { pacienteId = ficha.PacienteId });
            }

            ficha.FechaActualizacion = DateTime.Now;

            _context.FichasMedicas.Add(ficha);
            await _context.SaveChangesAsync();

            return RedirectToAction("Ver", new { pacienteId = ficha.PacienteId });
        }
    }
}
