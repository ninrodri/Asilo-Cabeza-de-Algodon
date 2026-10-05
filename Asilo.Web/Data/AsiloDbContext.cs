using Asilo.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace Asilo.Web.Data
{
    public class AsiloDbContext : DbContext
    {
        public AsiloDbContext(DbContextOptions<AsiloDbContext> options)
            : base(options)
        {
        }
        public DbSet<Anciano> Pacientes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }

        public DbSet<Familiar> Familiares { get; set; }

    }
}
