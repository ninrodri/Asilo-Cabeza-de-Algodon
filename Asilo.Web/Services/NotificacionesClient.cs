using Asilo.Web.Models;
using System.Net.Http.Json;

namespace Asilo.Web.Services
{
    public class NotificacionesClient
    {
        private readonly HttpClient _httpClient;

        public NotificacionesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> EnviarNotificacion(SolicitudMedica solicitud)
        {
            var notificacion = new
            {
                Paciente = solicitud.Paciente?.NombreCompleto ?? "",
                Familiar = solicitud.Familiar,
                CorreoFamiliar = solicitud.CorreoFamiliar,

                MedicoReferido =
                    solicitud.MedicoEspecialista?.NombreUsuario ?? "",

                Especialidad =
                    solicitud.Especialidad ?? "",

                Enfermero =
                    solicitud.Enfermero?.NombreUsuario ?? "",

                FechaAtencion =
                    solicitud.FechaAtencion,

                Motivo =
                    solicitud.Motivo
            };

            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/notificaciones",
                notificacion
            );

            return respuesta.IsSuccessStatusCode;
        }
    }
}