using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Asilo.Web.Models;
using Asilo.Web.Services;
using Asilo.Web.Data;

namespace Asilo.Web.Controllers
{
    public class SolicitudesController : Controller
    {
        private readonly NotificacionesClient _notificacionesClient;
        private readonly AsiloDbContext _context;

        public SolicitudesController(
            NotificacionesClient notificacionesClient,
            AsiloDbContext context)
        {
            _notificacionesClient = notificacionesClient;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }


        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                .ToListAsync();

            ViewBag.Medicos = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u =>
                    u.Estado &&
                    u.Rol != null &&
                    (u.Rol.Nombre == "Médico general" ||
                     u.Rol.Nombre == "Médico especialista"))
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(SolicitudMedica solicitud)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Pacientes = await _context.Pacientes
                .OrderBy(p => p.NombreCompleto)
                 .ToListAsync();

                ViewBag.Medicos = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u =>
                        u.Estado &&
                        u.Rol != null &&
                        (u.Rol.Nombre == "Médico general" ||
                         u.Rol.Nombre == "Médico especialista"))
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync();

                return View(solicitud);
            }

            var paciente = await _context.Pacientes
                .Include(p => p.Familiares)
                .FirstOrDefaultAsync(p => p.Id == solicitud.PacienteId);

            if (paciente == null)
            {
                ModelState.AddModelError(
                    "",
                    "El paciente seleccionado no existe."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(solicitud);
            }

            var familiar = paciente.Familiares
                .FirstOrDefault(f => !string.IsNullOrWhiteSpace(f.Correo));

            if (familiar == null)
            {
                ModelState.AddModelError(
                    "",
                    "El paciente no tiene un familiar con correo registrado."
                );

                ViewBag.Pacientes = await _context.Pacientes
                    .OrderBy(p => p.NombreCompleto)
                    .ToListAsync();

                return View(solicitud);
            }

            solicitud.Paciente = paciente;
            solicitud.Familiar = familiar.NombreCompleto;
            solicitud.CorreoFamiliar = familiar.Correo;
            solicitud.FechaSolicitud = DateTime.Now;
            solicitud.Estado = "Pendiente";

            _context.SolicitudesMedicas.Add(solicitud);
            await _context.SaveChangesAsync();

            bool enviada = false;

            try
            {
                enviada = await _notificacionesClient
                    .EnviarNotificacion(solicitud);
            }
            catch (HttpRequestException)
            {
                enviada = false;
            }
            catch (TaskCanceledException)
            {
                enviada = false;
            }
            catch (Exception)
            {
                enviada = false;
            }

            if (!enviada)
            {
                TempData["Advertencia"] =
                    "La solicitud fue guardada correctamente, pero no fue posible enviar la notificación al familiar.";

                return RedirectToAction("Index");
            }

            TempData["Exito"] =
                "La solicitud fue guardada y la notificación fue enviada correctamente.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string estado)
        {
            var estadosPermitidos = new[]
            {
        "Pendiente",
        "Asignada",
        "Atendida",
        "Finalizada"
    };

            if (!estadosPermitidos.Contains(estado))
            {
                return BadRequest();
            }

            var solicitud = await _context.SolicitudesMedicas
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            solicitud.Estado = estado;

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "El estado de la solicitud se actualizó correctamente.";

            return RedirectToAction("Index");
        }


    }


}