using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class ExamenLaboratorio
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        [Display(Name = "Visita médica")]
        public int VisitaMedicaId { get; set; }

        [ForeignKey("VisitaMedicaId")]
        public VisitaMedica? VisitaMedica { get; set; }

        [Required(ErrorMessage = "El nombre del examen es obligatorio.")]
        [Display(Name = "Examen solicitado")]
        public string NombreExamen { get; set; } = "";

        [Display(Name = "Fecha de solicitud")]
        public DateTime FechaSolicitud { get; set; } = DateTime.Now;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Solicitado";

        [Display(Name = "Resultado")]
        public string Resultado { get; set; } = "";

        [Display(Name = "Fecha del resultado")]
        public DateTime? FechaResultado { get; set; }

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";
    }
}
