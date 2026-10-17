using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class Cobro
    {
        public int Id { get; set; }

        [Required]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        public int? VisitaMedicaId { get; set; }

        [ForeignKey("VisitaMedicaId")]
        public VisitaMedica? VisitaMedica { get; set; }

        [Display(Name = "Fecha del cobro")]
        public DateTime FechaCobro { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Subtotal")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Descuento Fundación (%)")]
        public decimal PorcentajeDescuento { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Descuento")]
        public decimal MontoDescuento { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Total a pagar")]
        public decimal Total { get; set; }

        [Display(Name = "Estado")]
        public string Estado { get; set; } = "Pendiente";

        public ICollection<DetalleCobro> Detalles { get; set; }
            = new List<DetalleCobro>();

        public ICollection<Pago> Pagos { get; set; }
            = new List<Pago>();
    }
}