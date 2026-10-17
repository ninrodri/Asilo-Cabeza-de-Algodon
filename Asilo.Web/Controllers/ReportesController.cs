using Asilo.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class ReportesController : Controller
    {
        private readonly AsiloDbContext _context;

        public ReportesController(AsiloDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> CostosPorCita(int? pacienteId)
        {
            var consulta = _context.Cobros
                .Include(c => c.Paciente)
                .Include(c => c.VisitaMedica)
                .Include(c => c.Detalles)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.PacienteId == pacienteId.Value);
            }

            var cobros = await consulta
                .Where(c => c.VisitaMedicaId != null)
                .OrderByDescending(c => c.FechaCobro)
                .ToListAsync();

            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;

            return View(cobros);
        }

        [HttpGet]
        public async Task<IActionResult> AnalisisMedico(int? pacienteId)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;

            if (!pacienteId.HasValue)
            {
                return View(null);
            }

            var paciente = await _context.Pacientes
                .Include(p => p.FichaMedica)
                .Include(p => p.HistorialMedico)
                .Include(p => p.VisitasMedicas)
                    .ThenInclude(v => v.Receta)
                        .ThenInclude(r => r!.Detalles)
                .FirstOrDefaultAsync(p => p.Id == pacienteId.Value);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }

        [HttpGet]
        public async Task<IActionResult> CobrosPorPaciente(
    int? pacienteId,
    DateTime? fechaInicio,
    DateTime? fechaFin)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            var consulta = _context.Cobros
                .Include(c => c.Paciente)
                .Include(c => c.Detalles)
                .Include(c => c.Pagos)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.PacienteId == pacienteId.Value);
            }

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.FechaCobro.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.FechaCobro.Date <= fechaFin.Value.Date);
            }

            var cobros = await consulta
                .OrderByDescending(c => c.FechaCobro)
                .ToListAsync();

            return View(cobros);
        }

        [HttpGet]
        public async Task<IActionResult> PagosFundacion(
    DateTime? fechaInicio,
    DateTime? fechaFin)
        {
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            var consulta = _context.Pagos
                .Include(p => p.Cobro)
                    .ThenInclude(c => c!.Paciente)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.FechaPago.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(p =>
                    p.FechaPago.Date <= fechaFin.Value.Date);
            }

            var pagos = await consulta
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            return View(pagos);
        }

        [HttpGet]
        public async Task<IActionResult> Entradas(
    DateTime? fechaInicio,
    DateTime? fechaFin)
        {
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            var donacionesConsulta = _context.Donaciones
                .AsQueryable();

            var pagosConsulta = _context.Pagos
                .Include(p => p.Cobro)
                    .ThenInclude(c => c!.Paciente)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                donacionesConsulta = donacionesConsulta.Where(d =>
                    d.FechaDonacion.Date >= fechaInicio.Value.Date);

                pagosConsulta = pagosConsulta.Where(p =>
                    p.FechaPago.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                donacionesConsulta = donacionesConsulta.Where(d =>
                    d.FechaDonacion.Date <= fechaFin.Value.Date);

                pagosConsulta = pagosConsulta.Where(p =>
                    p.FechaPago.Date <= fechaFin.Value.Date);
            }

            var donaciones = await donacionesConsulta
                .OrderByDescending(d => d.FechaDonacion)
                .ToListAsync();

            var pagos = await pagosConsulta
                .OrderByDescending(p => p.FechaPago)
                .ToListAsync();

            ViewBag.Donaciones = donaciones;
            ViewBag.Pagos = pagos;

            ViewBag.TotalDonaciones = donaciones.Sum(d => d.Monto);
            ViewBag.TotalPagos = pagos.Sum(p => p.Monto);

            ViewBag.TotalEntradas =
                donaciones.Sum(d => d.Monto) +
                pagos.Sum(p => p.Monto);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExamenesPorPaciente(
    int? pacienteId,
    DateTime? fechaInicio,
    DateTime? fechaFin)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;
            ViewBag.FechaInicio = fechaInicio?.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fechaFin?.ToString("yyyy-MM-dd");

            var consulta = _context.ExamenesLaboratorio
                .Include(e => e.Paciente)
                .Include(e => e.VisitaMedica)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.PacienteId == pacienteId.Value);
            }

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.FechaSolicitud.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(e =>
                    e.FechaSolicitud.Date <= fechaFin.Value.Date);
            }

            var examenes = await consulta
                .OrderByDescending(e => e.FechaSolicitud)
                .ToListAsync();

            return View(examenes);
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }
    }
}