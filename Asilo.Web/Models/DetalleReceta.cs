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


        // =====================================================
        // MEDICAMENTO
        // =====================================================

        [Required(ErrorMessage = "El medicamento es obligatorio.")]
        [Display(Name = "Medicamento")]
        public string Medicamento { get; set; } = "";


        // =====================================================
        // FORMA FARMACÉUTICA
        // =====================================================

        [Required(ErrorMessage =
            "Debe seleccionar la forma farmacéutica.")]
        [Display(Name = "Forma farmacéutica")]
        public string FormaFarmaceutica { get; set; } = "";


        // =====================================================
        // DOSIFICACIÓN
        // =====================================================

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Cantidad por toma")]
        public decimal DosisPorToma { get; set; }


        [Required]
        [Display(Name = "Unidad de la dosis")]
        public string UnidadDosis { get; set; } = "";


        [Required]
        [Display(Name = "Tomas por día")]
        public int TomasPorDia { get; set; }


        [Required]
        [Display(Name = "Duración en días")]
        public int DuracionDias { get; set; }


        // =====================================================
        // PRESENTACIÓN DEL PRODUCTO
        // =====================================================

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Contenido por presentación")]
        public decimal ContenidoPresentacion { get; set; }


        [Required]
        [Display(Name = "Unidad del contenido")]
        public string UnidadPresentacion { get; set; } = "";


        [Required]
        [Display(Name = "Presentación")]
        public string Presentacion { get; set; } = "";


        // =====================================================
        // CAMPOS ANTERIORES
        // Los conservamos por compatibilidad.
        // =====================================================

        [Display(Name = "Dosis")]
        public string Dosis { get; set; } = "";

        [Display(Name = "Frecuencia")]
        public string Frecuencia { get; set; } = "";

        [Display(Name = "Duración")]
        public string Duracion { get; set; } = "";


        [Display(Name = "Indicaciones")]
        public string Indicaciones { get; set; } = "";


        // =====================================================
        // FARMACIA
        // =====================================================

        [Display(Name = "Cantidad entregada")]
        public int Cantidad { get; set; }


        [Column(TypeName = "decimal(10,2)")]
        [Display(Name = "Precio unitario")]
        public decimal PrecioUnitario { get; set; }


        // =====================================================
        // CÁLCULOS
        // =====================================================

        [NotMapped]
        [Display(Name = "Cantidad total necesaria")]
        public decimal CantidadTotalNecesaria
        {
            get
            {
                if (DosisPorToma <= 0 ||
                    TomasPorDia <= 0 ||
                    DuracionDias <= 0)
                {
                    return 0;
                }

                return DosisPorToma
                    * TomasPorDia
                    * DuracionDias;
            }
        }


        [NotMapped]
        [Display(Name = "Cantidad sugerida")]
        public int CantidadSugerida
        {
            get
            {
                if (ContenidoPresentacion <= 0)
                {
                    return 0;
                }

                return (int)Math.Ceiling(
                    CantidadTotalNecesaria /
                    ContenidoPresentacion
                );
            }
        }


        [NotMapped]
        [Display(Name = "Subtotal")]
        public decimal Subtotal
        {
            get
            {
                return Cantidad * PrecioUnitario;
            }
        }
    }
}