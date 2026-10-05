using System.ComponentModel.DataAnnotations;

namespace Asilo.Web.Models
{
    public class Rol
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = "";

        public ICollection<Usuario> Usuarios { get; set; }
            = new List<Usuario>();
    }
}
