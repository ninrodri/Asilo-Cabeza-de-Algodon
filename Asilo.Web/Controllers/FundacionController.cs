using Asilo.Web.Data;
using Asilo.Web.Models;
using Asilo.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Controllers
{
    [Authorize(Roles = "Administrador,Fundación")]
    public class FundacionController : Controller
    {
        private readonly AsiloDbContext _context;
        private readonly NotificacionesClient _notificacionesClient;

        public FundacionController(
            AsiloDbContext context,
            NotificacionesClient notificacionesClient)
        {
            _context = context;
            _notificacionesClient = notificacionesClient;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var solicitudes = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .Include(s => s.MedicoEspecialista)
                .Include(s => s.Enfermero)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();

            return View(solicitudes);
        }

        [HttpGet]
        public async Task<IActionResult> Asignar(int id)
        {
            var solicitud = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitud == null)
            {
                return NotFound();
            }

            ViewBag.MedicosEspecialistas = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u =>
                    u.Estado &&
                    u.Rol != null &&
                    u.Rol.Nombre == "Médico especialista")
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            ViewBag.Enfermeros = await _context.Usuarios
                .Include(u => u.Rol)
                .Where(u =>
                    u.Estado &&
                    u.Rol != null &&
                    u.Rol.Nombre == "Enfermero")
                .OrderBy(u => u.NombreUsuario)
                .ToListAsync();

            return View(solicitud);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(SolicitudMedica solicitud)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MedicosEspecialistas = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u =>
                        u.Estado &&
                        u.Rol != null &&
                        u.Rol.Nombre == "Médico especialista")
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync();

                ViewBag.Enfermeros = await _context.Usuarios
                    .Include(u => u.Rol)
                    .Where(u =>
                        u.Estado &&
                        u.Rol != null &&
                        u.Rol.Nombre == "Enfermero")
                    .OrderBy(u => u.NombreUsuario)
                    .ToListAsync();

                return View(solicitud);
            }

            var solicitudExistente = await _context.SolicitudesMedicas
                .Include(s => s.Paciente)
                    .ThenInclude(p => p.Familiares)
                .Include(s => s.MedicoEspecialista)
                .Include(s => s.Enfermero)
                .FirstOrDefaultAsync(s => s.Id == solicitud.Id);

            if (solicitudExistente == null)
            {
                return NotFound();
            }

            solicitudExistente.Especialidad = solicitud.Especialidad;
            solicitudExistente.MedicoEspecialistaId =
                solicitud.MedicoEspecialistaId;
            solicitudExistente.EnfermeroId =
                solicitud.EnfermeroId;
            solicitudExistente.FechaAtencion =
                solicitud.FechaAtencion;

            solicitudExistente.Estado = "Programada";

            await _context.SaveChangesAsync();

            // Recargar relaciones ya actualizadas
            await _context.Entry(solicitudExistente)
                .Reference(s => s.MedicoEspecialista)
                .LoadAsync();

            await _context.Entry(solicitudExistente)
                .Reference(s => s.Enfermero)
                .LoadAsync();

            var familiar = solicitudExistente.Paciente?
                .Familiares
                .FirstOrDefault(f =>
                    !string.IsNullOrWhiteSpace(f.Correo));

            if (familiar != null)
            {
                solicitudExistente.Familiar =
                    familiar.NombreCompleto;

                solicitudExistente.CorreoFamiliar =
                    familiar.Correo;

                try
                {
                    await _notificacionesClient
                        .EnviarNotificacion(solicitudExistente);
                }
                catch
                {
                    TempData["Advertencia"] =
                        "La solicitud fue programada, pero no fue posible enviar la notificación al familiar.";
                }
            }

            TempData["Exito"] =
                "La solicitud fue programada correctamente.";

            return RedirectToAction("Index");
        }
    }
}
