using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class Pago
    {
        public int Id { get; set; }

        [Required]
        public int CobroId { get; set; }

        [ForeignKey("CobroId")]
        public Cobro? Cobro { get; set; }

        [Display(Name = "Fecha de pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto pagado")]
        public decimal Monto { get; set; }

        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; } = "";

        [Display(Name = "Referencia")]
        public string Referencia { get; set; } = "";

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";
    }
}