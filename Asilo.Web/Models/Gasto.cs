using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class Gasto
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Categoría")]
        public string Categoria { get; set; } = "";

        [Required]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";

        [Required]
        [Display(Name = "Fecha del gasto")]
        public DateTime FechaGasto { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Display(Name = "Proveedor o beneficiario")]
        public string Proveedor { get; set; } = "";

        [Display(Name = "Referencia")]
        public string Referencia { get; set; } = "";
    }
}
