using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class CuotaMensual
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required]
        [Display(Name = "Mes")]
        public int Mes { get; set; }

        [Required]
        [Display(Name = "Año")]
        public int Anio { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Display(Name = "Fecha de generación")]
        public DateTime FechaGeneracion { get; set; } = DateTime.Now;

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        public int? CobroId { get; set; }

        [ForeignKey("CobroId")]
        public Cobro? Cobro { get; set; }
    }
}