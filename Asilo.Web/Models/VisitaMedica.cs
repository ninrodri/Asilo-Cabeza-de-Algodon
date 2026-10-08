using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class VisitaMedica
    {
        public int Id { get; set; }

        [Required]
        public int SolicitudMedicaId { get; set; }

        [ForeignKey("SolicitudMedicaId")]
        public SolicitudMedica? SolicitudMedica { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        [Display(Name = "Fecha de visita")]
        public DateTime FechaVisita { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Médico")]
        public string Medico { get; set; } = "";

        [Required]
        [Display(Name = "Diagnóstico")]
        public string Diagnostico { get; set; } = "";

        [Display(Name = "Tratamiento")]
        public string Tratamiento { get; set; } = "";

        [Display(Name = "Indicaciones")]
        public string Indicaciones { get; set; } = "";

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";
        public ICollection<ExamenLaboratorio> ExamenesLaboratorio { get; set; }
         = new List<ExamenLaboratorio>();
        public Receta? Receta { get; set; }
    }
}