using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class Anciano
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [Display(Name = "Nombre completo")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [Display(Name = "Fecha de nacimiento")]
        [DataType(DataType.Date)]
        public DateTime? FechaNacimiento { get; set; }

        [Required(ErrorMessage = "Debe seleccionar el sexo.")]
        [Display(Name = "Sexo")]
        public string Sexo { get; set; } = "";

        [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
        [Display(Name = "Fecha de ingreso")]
        [DataType(DataType.Date)]
        public DateTime? FechaIngreso { get; set; }

        [Required(ErrorMessage = "El motivo de ingreso es obligatorio.")]
        [Display(Name = "Motivo de ingreso")]
        public string MotivoIngreso { get; set; } = "";

        [Required(ErrorMessage = "El nombre del familiar es obligatorio.")]
        [Display(Name = "Familiar responsable")]
        public string FamiliarResponsable { get; set; } = "";

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [Display(Name = "Teléfono del familiar")]
        public string TelefonoFamiliar { get; set; } = "";

        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        [Display(Name = "Correo del familiar")]
        public string CorreoFamiliar { get; set; } = "";

        [Display(Name = "Padecimientos")]
        public string Padecimientos { get; set; } = "";

        [Display(Name = "Medicamentos actuales")]
        public string MedicamentosActuales { get; set; } = "";

        public ICollection<Familiar> Familiares { get; set; }
        = new List<Familiar>();
        public ICollection<HistorialMedico> HistorialMedico { get; set; }
        = new List<HistorialMedico>();
        public ICollection<SolicitudMedica> SolicitudesMedicas { get; set; }
        = new List<SolicitudMedica>();
        public FichaMedica? FichaMedica { get; set; }

        public ICollection<VisitaMedica> VisitasMedicas { get; set; }
        = new List<VisitaMedica>();
        public ICollection<ExamenLaboratorio> ExamenesLaboratorio { get; set; }
        = new List<ExamenLaboratorio>();
        public ICollection<Receta> Recetas { get; set; }
        = new List<Receta>();

    }
}
