using Microsoft.AspNetCore.Mvc;
using Asilo.Web.Models;
using Asilo.Web.Services;

namespace Asilo.Web.Controllers
{
    public class SolicitudesController : Controller
    {
        private readonly NotificacionesClient _notificacionesClient;

        public SolicitudesController(
            NotificacionesClient notificacionesClient)
        {
            _notificacionesClient = notificacionesClient;
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(
            SolicitudMedica solicitud)
        {
            if (!ModelState.IsValid)
            {
                return View(solicitud);
            }

            bool enviada =
                await _notificacionesClient.EnviarNotificacion(
                    solicitud);

            if (!enviada)
            {
                ModelState.AddModelError(
                    "",
                    "No se pudo enviar la notificación al familiar."
                );

                return View(solicitud);
            }

            return View("Confirmacion", solicitud);

              }
        }
}
