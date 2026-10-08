using Asilo.Notificaciones.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace Asilo.Notificaciones.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public NotificacionesController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CrearNotificacion(Notificacion notificacion)
        {
            try
            {
                string smtpServer = _configuration["EmailSettings:SmtpServer"]!;
                int port = int.Parse(_configuration["EmailSettings:Port"]!);
                string senderEmail = _configuration["EmailSettings:SenderEmail"]!;
                string senderName = _configuration["EmailSettings:SenderName"]!;
                string password = _configuration["EmailSettings:Password"]!;

                var mensaje = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Notificación médica - Asilo Cabeza de Algodón",
                    Body = $@"
Estimado/a {notificacion.Familiar}:

Se informa que el paciente {notificacion.Paciente}
ha sido programado para atención médica.

Médico especialista:
{notificacion.MedicoReferido}

Especialidad:
{notificacion.Especialidad}

Enfermero acompañante:
{notificacion.Enfermero}

Fecha y hora de atención:
{notificacion.FechaAtencion?.ToString("dd/MM/yyyy HH:mm")}

Motivo:
{notificacion.Motivo}

Atentamente,
Asilo de Ancianos Cabeza de Algodón
"
                };

                mensaje.To.Add(notificacion.CorreoFamiliar);

                using var smtpClient = new SmtpClient(smtpServer, port)
                {
                    Credentials = new NetworkCredential(senderEmail, password),
                    EnableSsl = true
                };

                await smtpClient.SendMailAsync(mensaje);

                return Ok(new
                {
                    mensaje = "Correo enviado correctamente",
                    correo = notificacion.CorreoFamiliar
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = "Error al enviar el correo",
                    error = ex.Message
                });
            }
        }
    }
}