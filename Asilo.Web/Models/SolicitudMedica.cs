using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class SolicitudMedica
    {
        [Required]
        [Display(Name = "Nombre del paciente")]
        public string Paciente { get; set; } = "";

        [Required]
        [Display(Name = "Nombre del familiar")]
        public string Familiar { get; set; } = "";

        [Required]
        [EmailAddress]
        [Display(Name = "Correo del familiar")]
        public string CorreoFamiliar { get; set; } = "";

        [Required]
        [Display(Name = "Médico referido")]
        public string MedicoReferido { get; set; } = "";

        [Required]
        [Display(Name = "Especialidad")]
        public string Especialidad { get; set; } = "";

        [Required]
        [Display(Name = "Motivo de la referencia")]
        public string Motivo { get; set; } = "";
    }
}
