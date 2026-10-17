using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class ConsultaGeneral
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        [Display(Name = "Fecha de consulta")]
        public DateTime FechaConsulta { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Médico general")]
        public string Medico { get; set; } = "";

        [Required]
        [Display(Name = "Motivo de consulta")]
        public string Motivo { get; set; } = "";

        [Required]
        [Display(Name = "Diagnóstico inicial")]
        public string Diagnostico { get; set; } = "";

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";

        [Display(Name = "¿Requiere especialista?")]
        public bool RequiereEspecialista { get; set; }

        [Display(Name = "Especialidad requerida")]
        public string? EspecialidadRequerida { get; set; }

        [Display(Name = "Motivo de referencia")]
        public string? MotivoReferencia { get; set; }
    }
}