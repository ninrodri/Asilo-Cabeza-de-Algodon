using Asilo.Web.Data;
using Asilo.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize]
    public class ReportesController : Controller
    {
        private readonly AsiloDbContext _context;

        public ReportesController(AsiloDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // MENÚ PRINCIPAL DE REPORTES
        // ============================================================

        [Authorize(Roles =
            "Administrador,Personal administrativo,Médico general,Médico especialista,Enfermero")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        // ============================================================
        // 1. COSTOS POR CITA Y PACIENTE
        // Administrador / Personal administrativo
        // ============================================================

        [Authorize(Roles = "Administrador,Personal administrativo")]
        [HttpGet]
        public async Task<IActionResult> CostosPorCita(int? pacienteId)
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.PacienteId = pacienteId;

            // Cobros relacionados con visitas médicas mediante CobroVisita.
            var idsCobrosConVisitas = _context.CobrosVisitas
                .Select(cv => cv.CobroId);

            var consulta = _context.Cobros
                .Include(c => c.Paciente)
                .Include(c => c.Detalles)
                .AsQueryable();

            if (pacienteId.HasValue)
            {
                consulta = consulta.Where(c =>
                    c.PacienteId == pacienteId.Value);
            }

            // Soporta tanto cobros antiguos como cobros agrupados actuales.
            consulta = consulta.Where(c =>
                c.VisitaMedicaId != null ||
                idsCobrosConVisitas.Contains(c.Id));

            var cobros = await consulta
                .OrderByDescending(c => c.FechaCobro)
                .ToListAsync();

            // ============================================================
            // ESTADÍSTICAS DEL REPORTE
            // ============================================================

            var totalConsultas = cobros
                .SelectMany(c => c.Detalles)
                .Where(d => d.Tipo == "Consulta médica")
                .Sum(d => d.Monto);

            var totalLaboratorio = cobros
                .SelectMany(c => c.Detalles)
                .Where(d => d.Tipo == "Laboratorio")
                .Sum(d => d.Monto);

            var totalMedicamentos = cobros
                .SelectMany(c => c.Detalles)
                .Where(d => d.Tipo == "Medicamento")
                .Sum(d => d.Monto);

            var totalGeneral = cobros
                .Sum(c => c.Total);

            ViewBag.TotalCitas = cobros.Count;
            ViewBag.TotalConsultas = totalConsultas;
            ViewBag.TotalLaboratorio = totalLaboratorio;
            ViewBag.TotalMedicamentos = totalMedicamentos;
            ViewBag.TotalGeneral = totalGeneral;

            return View(cobros);
        }

        // ============================================================
        // 2. ANÁLISIS MÉDICO POR PACIENTE
        // Administrador / Médico general / Especialista / Enfermero
        // ============================================================

        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista,Enfermero")]
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
                .FirstOrDefaultAsync(p =>
                    p.Id == pacienteId.Value);

            if (paciente == null)
            {
                return NotFound();
            }

            return View(paciente);
        }


        // ============================================================
        // 3. COBROS POR PACIENTE Y RANGO DE FECHAS
        // Administrador / Personal administrativo
        // ============================================================

        [Authorize(Roles = "Administrador,Personal administrativo")]
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
            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

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


        // ============================================================
        // 4. PAGOS A LA FUNDACIÓN
        // Administrador / Personal administrativo
        // ============================================================

        [Authorize(Roles = "Administrador,Personal administrativo")]
        [HttpGet]
        public async Task<IActionResult> PagosFundacion(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

            var consulta = _context
                .Set<PagoFundacion>()
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


        // ============================================================
        // 5. ENTRADAS: DONACIONES Y COBROS
        // Administrador / Personal administrativo
        // ============================================================

        [Authorize(Roles = "Administrador,Personal administrativo")]
        [HttpGet]
        public async Task<IActionResult> Entradas(
            DateTime? fechaInicio,
            DateTime? fechaFin)
        {
            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

            var donacionesConsulta = _context.Donaciones
                .AsQueryable();

            var pagosConsulta = _context.Pagos
                .Include(p => p.Cobro)
                    .ThenInclude(c => c!.Paciente)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                donacionesConsulta =
                    donacionesConsulta.Where(d =>
                        d.FechaDonacion.Date >= fechaInicio.Value.Date);

                pagosConsulta =
                    pagosConsulta.Where(p =>
                        p.FechaPago.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                donacionesConsulta =
                    donacionesConsulta.Where(d =>
                        d.FechaDonacion.Date <= fechaFin.Value.Date);

                pagosConsulta =
                    pagosConsulta.Where(p =>
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

            ViewBag.TotalDonaciones =
                donaciones.Sum(d => d.Monto);

            ViewBag.TotalPagos =
                pagos.Sum(p => p.Monto);

            ViewBag.TotalEntradas =
                ViewBag.TotalDonaciones +
                ViewBag.TotalPagos;

            return View();
        }


        // ============================================================
        // 6. EXÁMENES MÉDICOS POR PACIENTE
        // Administrador / Médico general / Médico especialista
        // ============================================================

        [Authorize(Roles =
            "Administrador,Médico general,Médico especialista")]
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

            ViewBag.FechaInicio =
                fechaInicio?.ToString("yyyy-MM-dd");

            ViewBag.FechaFin =
                fechaFin?.ToString("yyyy-MM-dd");

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

            var totalExamenes = examenes.Count;

            var totalCompletados = examenes.Count(e =>
                e.Estado == "Completado");

            var totalPendientes = examenes.Count(e =>
                e.Estado != "Completado");

            var costoTotal = examenes.Sum(e => e.Costo);

            ViewBag.TotalExamenes = totalExamenes;
            ViewBag.TotalCompletados = totalCompletados;
            ViewBag.TotalPendientes = totalPendientes;
            ViewBag.CostoTotalExamenes = costoTotal;

            return View(examenes);
        }


        // ============================================================
        // 7. MEDICAMENTOS POR PACIENTE
        //
        

        [Authorize(Roles =
    "Administrador,Médico general,Médico especialista,Enfermero")]
        [HttpGet]
        public async Task<IActionResult> MedicamentosPorPaciente(
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

            if (!pacienteId.HasValue)
            {
                ViewBag.SeleccionarPaciente = true;

                return View(new List<MedicamentoAplicado>());
            }

            ViewBag.SeleccionarPaciente = false;

            var consulta = _context.MedicamentosAplicados
                .Include(m => m.VisitaMedica)
                    .ThenInclude(v => v!.Paciente)
                .Include(m => m.RegistradoPor)
                .Where(m =>
                    m.VisitaMedica != null &&
                    m.VisitaMedica.PacienteId == pacienteId.Value)
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.FechaAplicacion.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                consulta = consulta.Where(m =>
                    m.FechaAplicacion.Date <= fechaFin.Value.Date);
            }

            var medicamentos = await consulta
                .OrderByDescending(m => m.FechaAplicacion)
                .ToListAsync();

            var resumenMedicamentos = medicamentos
    .GroupBy(m => m.Medicamento)
    .Select(g => new
    {
        Medicamento = g.Key,
        Cantidad = g.Count()
    })
    .OrderByDescending(x => x.Cantidad)
    .ToList();

            ViewBag.TotalAplicaciones = medicamentos.Count;

            ViewBag.MedicamentosLabels = resumenMedicamentos
                .Select(x => x.Medicamento)
                .ToList();

            ViewBag.MedicamentosCantidades = resumenMedicamentos
                .Select(x => x.Cantidad)
                .ToList();

            return View(medicamentos);
        }
    }
}