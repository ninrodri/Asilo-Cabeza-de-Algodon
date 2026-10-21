using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class PagoFundacion
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Fecha de pago")]
        public DateTime FechaPago { get; set; } = DateTime.Now;

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monto")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Monto { get; set; }

        [Required]
        [Display(Name = "Método de pago")]
        public string MetodoPago { get; set; } = "";

        [Required]
        [Display(Name = "Referencia")]
        public string Referencia { get; set; } = "";

        [Required]
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";

        [Required]
        [Display(Name = "Registrado por")]
        public string RegistradoPor { get; set; } = "";
    }
}