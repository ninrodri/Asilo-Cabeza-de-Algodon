using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class MedicamentoAplicado
    {
        public int Id { get; set; }

        [Required]
        public int VisitaMedicaId { get; set; }

        [ForeignKey("VisitaMedicaId")]
        public VisitaMedica? VisitaMedica { get; set; }

        [Required]
        [Display(Name = "Medicamento o tratamiento")]
        public string Medicamento { get; set; } = "";

        [Required]
        [Display(Name = "Cantidad aplicada")]
        public string CantidadAplicada { get; set; } = "";

        [Required]
        [Display(Name = "Tiempo de aplicación")]
        public string TiempoAplicacion { get; set; } = "";

        [Required]
        [Display(Name = "Fecha y hora de aplicación")]
        public DateTime FechaAplicacion { get; set; } = DateTime.Now;

        [Display(Name = "Observaciones")]
        public string? Observaciones { get; set; }

        [Required]
        public int RegistradoPorId { get; set; }

        [ForeignKey("RegistradoPorId")]
        public Usuario? RegistradoPor { get; set; }
    }
}