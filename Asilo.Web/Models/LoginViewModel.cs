using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Ingrese su usuario o correo.")]
        [Display(Name = "Usuario o correo")]
        public string UsuarioOCorreo { get; set; } = "";

        [Required(ErrorMessage = "Ingrese su contraseña.")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = "";
    }
}