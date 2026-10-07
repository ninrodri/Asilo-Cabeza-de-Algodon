using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asilo.Web.Models
{
    public class HistorialMedico
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Paciente")]
        public int PacienteId { get; set; }

        [ForeignKey("PacienteId")]
        public Anciano? Paciente { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Display(Name = "Fecha")]
        [DataType(DataType.DateTime)]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El tipo de registro es obligatorio.")]
        [Display(Name = "Tipo de registro")]
        public string TipoRegistro { get; set; } = "";

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; } = "";

        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = "";

        [Display(Name = "Profesional responsable")]
        public string ProfesionalResponsable { get; set; } = "";
    }
}
