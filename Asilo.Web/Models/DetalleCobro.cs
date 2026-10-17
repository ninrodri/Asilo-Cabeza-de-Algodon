using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class DetalleCobro
    {
        public int Id { get; set; }

        [Required]
        public int CobroId { get; set; }

        [ForeignKey("CobroId")]
        public Cobro? Cobro { get; set; }

        [Required]
        [Display(Name = "Tipo de cargo")]
        public string Tipo { get; set; } = "";

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";

        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }
    }
}