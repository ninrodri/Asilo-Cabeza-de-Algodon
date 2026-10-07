using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class FichaMedica
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Display(Name = "Padecimientos")]
        public string Padecimientos { get; set; } = "";

        [Display(Name = "Enfermedades")]
        public string Enfermedades { get; set; } = "";

        [Display(Name = "Psicopatologías")]
        public string Psicopatologias { get; set; } = "";

        [Display(Name = "Medicamentos frecuentes")]
        public string MedicamentosFrecuentes { get; set; } = "";

        [Display(Name = "Alergias")]
        public string Alergias { get; set; } = "";

        [Display(Name = "Observaciones médicas")]
        public string ObservacionesMedicas { get; set; } = "";

        [Display(Name = "Fecha de actualización")]
        public DateTime FechaActualizacion { get; set; } = DateTime.Now;
    }
}