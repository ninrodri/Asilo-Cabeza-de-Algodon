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
        [Display(Name = "Motivo de referencia")]
        public string Motivo { get; set; } = "";

        [Display(Name = "Observaciones del médico general")]
        public string Observaciones { get; set; } = "";

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente de Fundación";

        // Datos asignados posteriormente por Fundación

        [Display(Name = "Especialidad")]
        public string? Especialidad { get; set; }

        [Display(Name = "Médico especialista")]
        public int? MedicoEspecialistaId { get; set; }

        [ForeignKey("MedicoEspecialistaId")]
        public Usuario? MedicoEspecialista { get; set; }

        [Display(Name = "Enfermero acompañante")]
        public int? EnfermeroId { get; set; }

        [ForeignKey("EnfermeroId")]
        public Usuario? Enfermero { get; set; }

        [Display(Name = "Fecha y hora de atención")]
        public DateTime? FechaAtencion { get; set; }

        // Datos auxiliares para notificaciones
        [NotMapped]
        [Display(Name = "Nombre del familiar")]
        public string Familiar { get; set; } = "";

        [NotMapped]
        [Display(Name = "Correo del familiar")]
        public string CorreoFamiliar { get; set; } = "";

        public VisitaMedica? VisitaMedica { get; set; }
    }
}