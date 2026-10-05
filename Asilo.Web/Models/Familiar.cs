using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class Familiar
    {
        public int Id { get; set; }

        [Required]
        public string NombreCompleto { get; set; } = "";

        [Required]
        public string Parentesco { get; set; } = "";

        [Required]
        public string Telefono { get; set; } = "";

        [EmailAddress]
        public string Correo { get; set; } = "";

        public string Direccion { get; set; } = "";

        [Required]
        public int PacienteId { get; set; }

        public Anciano? Paciente { get; set; }
    }
}
