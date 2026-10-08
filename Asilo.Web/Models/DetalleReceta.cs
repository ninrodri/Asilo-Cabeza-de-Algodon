using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class DetalleReceta
    {
        public int Id { get; set; }

        [Required]
        public int RecetaId { get; set; }

        [ForeignKey("RecetaId")]
        public Receta? Receta { get; set; }

        [Required(ErrorMessage = "El medicamento es obligatorio.")]
        [Display(Name = "Medicamento")]
        public string Medicamento { get; set; } = "";

        [Required(ErrorMessage = "La dosis es obligatoria.")]
        [Display(Name = "Dosis")]
        public string Dosis { get; set; } = "";

        [Required(ErrorMessage = "La frecuencia es obligatoria.")]
        [Display(Name = "Frecuencia")]
        public string Frecuencia { get; set; } = "";

        [Display(Name = "Duración")]
        public string Duracion { get; set; } = "";

        [Display(Name = "Indicaciones")]
        public string Indicaciones { get; set; } = "";
    }
}