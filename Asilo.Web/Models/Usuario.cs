using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Nombre de usuario")]
        public string NombreUsuario { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public bool Estado { get; set; } = true;

        public int RolId { get; set; }

        public Rol? Rol { get; set; }
    }
}