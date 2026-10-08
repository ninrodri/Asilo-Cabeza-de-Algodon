using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class Receta
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        public int VisitaMedicaId { get; set; }

        [ForeignKey("VisitaMedicaId")]
        public VisitaMedica? VisitaMedica { get; set; }

        [Required]
        [Display(Name = "Fecha de receta")]
        public DateTime FechaReceta { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Médico")]
        public string Medico { get; set; } = "";

        [Display(Name = "Indicaciones generales")]
        public string IndicacionesGenerales { get; set; } = "";

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        public ICollection<DetalleReceta> Detalles { get; set; }
            = new List<DetalleReceta>();
    }
}