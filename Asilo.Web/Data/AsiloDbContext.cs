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

        public DbSet<FichaMedica> FichasMedicas { get; set; }
        public DbSet<HistorialMedico> HistorialesMedicos { get; set; }
        public DbSet<SolicitudMedica> SolicitudesMedicas { get; set; }

        public DbSet<VisitaMedica> VisitasMedicas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FichaMedica>()
                .HasOne(f => f.Paciente)
                .WithOne(p => p.FichaMedica)
                .HasForeignKey<FichaMedica>(f => f.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<HistorialMedico>()
                .HasOne(h => h.Paciente)
                .WithMany(p => p.HistorialMedico)
                .HasForeignKey(h => h.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SolicitudMedica>()
                .HasOne(s => s.Paciente)
                .WithMany(p => p.SolicitudesMedicas)
                .HasForeignKey(s => s.PacienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitaMedica>()
                 .HasOne(v => v.SolicitudMedica)
                 .WithOne(s => s.VisitaMedica)
                 .HasForeignKey<VisitaMedica>(v => v.SolicitudMedicaId)
                 .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VisitaMedica>()
                .HasOne(v => v.Paciente)
                .WithMany(p => p.VisitasMedicas)
                .HasForeignKey(v => v.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
