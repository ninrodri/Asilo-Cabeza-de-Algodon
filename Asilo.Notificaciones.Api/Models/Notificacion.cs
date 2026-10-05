namespace Asilo.Notificaciones.Api.Models
{
    public class Notificacion
    {
        public string Paciente { get; set; } = "";

        public string Familiar { get; set; } = "";

        public string CorreoFamiliar { get; set; } = "";

        public string MedicoReferido { get; set; } = "";

        public string Especialidad { get; set; } = "";

        public string Motivo { get; set; } = "";
    }
}
