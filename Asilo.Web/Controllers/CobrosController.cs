using Asilo.Web.Data;
using Asilo.Web.Models;
using Asilo.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Personal administrativo")]
    public class CobrosController : Controller
    {
        private readonly AsiloDbContext _context;

        public CobrosController(AsiloDbContext context)
        {
            _context = context;
        }


        // =========================================================
        // COBROS PENDIENTES + COBROS GENERADOS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Visitas que todavía no han sido incluidas
            // en ningún cobro.
            var visitasPendientes = await _context.VisitasMedicas

                .Include(v => v.Paciente)
                    .ThenInclude(p => p.Familiares)

                .Include(v => v.ExamenesLaboratorio)

                .Include(v => v.Receta)
                    .ThenInclude(r => r!.Detalles)

                .Where(v =>

                    // Compatibilidad con cobros antiguos
                    !_context.Cobros.Any(c =>
                        c.VisitaMedicaId == v.Id)

                    &&

                    // Nueva relación muchos-a-muchos
                    !_context.CobrosVisitas.Any(cv =>
                        cv.VisitaMedicaId == v.Id)
                )

                .ToListAsync();


            // Solo tomamos atenciones que ya pueden cobrarse
            var visitasListas = visitasPendientes
                .Where(v =>
                {
                    var laboratorioTerminado =
                        !v.ExamenesLaboratorio.Any()
                        ||
                        v.ExamenesLaboratorio.All(e =>
                            e.Estado == "Completado");

                    var farmaciaTerminada =
                        v.Receta == null
                        ||
                        v.Receta.Estado == "Entregada";

                    return laboratorioTerminado
                           && farmaciaTerminada;
                })
                .ToList();


            // AGRUPAR POR PACIENTE
            var pendientes = visitasListas

                .GroupBy(v => v.PacienteId)

                .Select(grupo =>
                {
                    var primeraVisita = grupo.First();

                    var paciente =
                        primeraVisita.Paciente;

                    var familiar =
                        paciente?.Familiares
                            .FirstOrDefault();

                    var nombreFamiliar =
                        familiar?.NombreCompleto
                        ?? paciente?.FamiliarResponsable
                        ?? "No registrado";


                    // Sumar todas las consultas pendientes
                    var costoConsultas =
                        grupo.Sum(v =>
                            v.CostoConsulta);


                    // Sumar todos los laboratorios
                    var costoLaboratorio =
                        grupo.Sum(v =>
                            v.ExamenesLaboratorio
                                .Where(e =>
                                    e.Estado == "Completado")
                                .Sum(e => e.Costo));


                    // Sumar medicamentos de todas las recetas
                    var costoMedicamentos =
                        grupo.Sum(v =>
                            v.Receta?.Detalles
                                .Sum(d => d.Subtotal)
                            ?? 0);


                    return new CobroPendienteViewModel
                    {
                        PacienteId =
                            grupo.Key,

                        Paciente =
                            paciente?.NombreCompleto
                            ?? "Paciente",

                        FamiliarResponsable =
                            nombreFamiliar,

                        CantidadAtenciones =
                            grupo.Count(),

                        CostoConsulta =
                            costoConsultas,

                        CostoLaboratorio =
                            costoLaboratorio,

                        CostoMedicamentos =
                            costoMedicamentos
                    };
                })

                // No mostrar pacientes cuyo total pendiente sea 0
                .Where(p => p.Subtotal > 0)

                .OrderBy(p => p.Paciente)

                .ToList();


            // Cobros que ya fueron generados
            var generados = await _context.Cobros

                .Include(c => c.Paciente)

                .Include(c => c.Pagos)

                .OrderByDescending(c =>
                    c.FechaCobro)

                .ToListAsync();


            var model = new CobrosIndexViewModel
            {
                Pendientes = pendientes,
                Generados = generados
            };


            return View(model);
        }


        // =========================================================
        // GENERAR RECIBO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generar(
     int pacienteId,
     decimal porcentajeDescuento)
        {
            if (porcentajeDescuento < 0 ||
                porcentajeDescuento > 100)
            {
                TempData["Advertencia"] =
                    "El descuento debe estar entre 0 y 100.";

                return RedirectToAction("Index");
            }


            // Buscar todas las visitas pendientes de cobro del paciente
            var visitas = await _context.VisitasMedicas

                .Include(v => v.Paciente)

                .Include(v => v.ExamenesLaboratorio)

                .Include(v => v.Receta)
                    .ThenInclude(r => r!.Detalles)

                .Where(v =>
                    v.PacienteId == pacienteId

                    &&

                    !_context.Cobros.Any(c =>
                        c.VisitaMedicaId == v.Id)

                    &&

                    !_context.CobrosVisitas.Any(cv =>
                        cv.VisitaMedicaId == v.Id)
                )

                .OrderBy(v => v.FechaVisita)

                .ToListAsync();

            var visitasListas = visitas
    .Where(v =>
    {
        var laboratorioTerminado =
            !v.ExamenesLaboratorio.Any()
            ||
            v.ExamenesLaboratorio.All(e =>
                e.Estado == "Completado");

        var farmaciaTerminada =
            v.Receta == null
            ||
            v.Receta.Estado == "Entregada";

        return laboratorioTerminado
               && farmaciaTerminada;
    })
    .ToList();

            if (!visitasListas.Any())
            {
                TempData["Advertencia"] =
                    "No existen atenciones finalizadas pendientes de cobro para este interno.";

                return RedirectToAction("Index");
            }


            var cobro = new Cobro
            {
                PacienteId = pacienteId,

                // Ya no pertenece a una sola visita
                VisitaMedicaId = null,

                FechaCobro = DateTime.Now,

                PorcentajeDescuento =
                    porcentajeDescuento,

                Estado = "Pendiente"
            };


            // CONSULTAS
            foreach (var visita in visitasListas)
            {
                if (visita.CostoConsulta > 0)
                {
                    cobro.Detalles.Add(
                        new DetalleCobro
                        {
                            Tipo = "Consulta médica",

                            Descripcion =
                         $"Consulta médica #{visita.Id} - " +
                         $"{visita.Medico} - " +
                         $"{visita.FechaVisita:dd/MM/yyyy}",

                            Monto =
                                visita.CostoConsulta
                        }
                    );
                }
            }


            // LABORATORIO
            foreach (var visita in visitasListas)
            {
                foreach (var examen
                         in visita.ExamenesLaboratorio)
                {
                    if (examen.Estado == "Completado" &&
                        examen.Costo > 0)
                    {
                        cobro.Detalles.Add(
                            new DetalleCobro
                            {
                                Tipo = "Laboratorio",

                                Descripcion =
                                    examen.NombreExamen,

                                Monto =
                                    examen.Costo
                            }
                        );
                    }
                }
            }


            // MEDICAMENTOS
            foreach (var visita in visitasListas)
            {
                if (visita.Receta == null)
                {
                    continue;
                }

                foreach (var medicamento
                         in visita.Receta.Detalles)
                {
                    if (medicamento.Subtotal > 0)
                    {
                        cobro.Detalles.Add(
                            new DetalleCobro
                            {
                                Tipo = "Medicamento",

                                Descripcion =
                                    $"{medicamento.Medicamento} " +
                                    $"x {medicamento.Cantidad}",

                                Monto =
                                    medicamento.Subtotal
                            }
                        );
                    }
                }
            }


            cobro.Subtotal =
                cobro.Detalles.Sum(d => d.Monto);


            if (cobro.Subtotal <= 0)
            {
                TempData["Advertencia"] =
                    "No existen cargos con valor para generar el recibo.";

                return RedirectToAction("Index");
            }


            cobro.MontoDescuento =
                cobro.Subtotal *
                (porcentajeDescuento / 100);


            cobro.Total =
                cobro.Subtotal -
                cobro.MontoDescuento;


            _context.Cobros.Add(cobro);


            // Asociar todas las visitas al mismo cobro
            foreach (var visita in visitasListas)
            {
                _context.CobrosVisitas.Add(
                    new CobroVisita
                    {
                        Cobro = cobro,
                        VisitaMedicaId = visita.Id
                    }
                );
            }


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El recibo fue generado correctamente.";


            return RedirectToAction(
                "Detalle",
                new { id = cobro.Id }
            );
        }


        // =========================================================
        // REGISTRAR PAGO
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarPago(
            int cobroId,
            decimal monto,
            string metodoPago,
            string referencia,
            string observaciones)
        {
            var cobro = await _context.Cobros
                .Include(c => c.Pagos)
                .FirstOrDefaultAsync(c =>
                    c.Id == cobroId);


            if (cobro == null)
            {
                return NotFound();
            }


            var totalPagado =
                cobro.Pagos.Sum(p => p.Monto);


            var saldoPendiente =
                cobro.Total - totalPagado;


            if (monto <= 0)
            {
                TempData["Advertencia"] =
                    "El monto del pago debe ser mayor que cero.";

                return RedirectToAction(
                    "Detalle",
                    new { id = cobroId }
                );
            }


            if (monto > saldoPendiente)
            {
                TempData["Advertencia"] =
                    "El pago no puede ser mayor que el saldo pendiente.";

                return RedirectToAction(
                    "Detalle",
                    new { id = cobroId }
                );
            }


            var pago = new Pago
            {
                CobroId = cobro.Id,

                FechaPago = DateTime.Now,

                Monto = monto,

                MetodoPago =
                    metodoPago ?? "",

                Referencia =
                    referencia ?? "",

                Observaciones =
                    observaciones ?? ""
            };


            _context.Pagos.Add(pago);


            var nuevoTotalPagado =
                totalPagado + monto;


            if (nuevoTotalPagado < cobro.Total)
            {
                cobro.Estado = "Parcial";
            }
            else
            {
                cobro.Estado = "Pagado";
            }


            await _context.SaveChangesAsync();


            TempData["Exito"] =
                "El pago fue registrado correctamente.";


            return RedirectToAction(
                "Detalle",
                new { id = cobroId }
            );
        }


        // =========================================================
        // DETALLE DEL COBRO
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var cobro = await _context.Cobros

                .Include(c => c.Paciente)
                    .ThenInclude(p => p.Familiares)

                .Include(c => c.VisitaMedica)

                .Include(c => c.Detalles)

                .Include(c => c.Pagos)

                .FirstOrDefaultAsync(c =>
                    c.Id == id);


            if (cobro == null)
            {
                return NotFound();
            }


            return View(cobro);
        }
    }
}