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
            var respuesta = await _httpClient.PostAsJsonAsync(
                "api/notificaciones",
                solicitud
            );

            return respuesta.IsSuccessStatusCode;
        }
    }
}
