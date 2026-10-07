using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class SolicitudMedica
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Médico referido")]
        public string MedicoReferido { get; set; } = "";

        [Required]
        [Display(Name = "Especialidad")]
        public string Especialidad { get; set; } = "";

        [Required]
        [Display(Name = "Motivo de la referencia")]
        public string Motivo { get; set; } = "";

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";

        [NotMapped]
        [Display(Name = "Nombre del familiar")]
        public string Familiar { get; set; } = "";

        [NotMapped]
        [Display(Name = "Correo del familiar")]
        public string CorreoFamiliar { get; set; } = "";

        public VisitaMedica? VisitaMedica { get; set; }

    }
}