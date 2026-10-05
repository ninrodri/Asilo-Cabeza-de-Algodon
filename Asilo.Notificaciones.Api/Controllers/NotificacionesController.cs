using Asilo.Notificaciones.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace Asilo.Notificaciones.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        [HttpPost]
        public IActionResult CrearNotificacion(Notificacion notificacion)
        {
            Console.WriteLine("==================================");
            Console.WriteLine("NUEVA NOTIFICACIÓN");
            Console.WriteLine("==================================");
            Console.WriteLine($"Paciente: {notificacion.Paciente}");
            Console.WriteLine($"Familiar: {notificacion.Familiar}");
            Console.WriteLine($"Correo: {notificacion.CorreoFamiliar}");
            Console.WriteLine($"Médico: {notificacion.MedicoReferido}");
            Console.WriteLine($"Especialidad: {notificacion.Especialidad}");
            Console.WriteLine($"Motivo: {notificacion.Motivo}");
            Console.WriteLine("==================================");

            return Ok(new
            {
                mensaje = "Notificación procesada correctamente",
                correo = notificacion.CorreoFamiliar
            });
        }
    }
}
