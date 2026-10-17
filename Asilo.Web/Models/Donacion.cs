using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class Donacion
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tipo de donante")]
        public string TipoDonante { get; set; } = "";

        [Required]
        [Display(Name = "Nombre del donante")]
        public string NombreDonante { get; set; } = "";

        [Required]
        [Display(Name = "Fecha de donación")]
        public DateTime FechaDonacion { get; set; } = DateTime.Now;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";
    }
}