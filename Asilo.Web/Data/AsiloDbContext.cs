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
        public DbSet<ExamenLaboratorio> ExamenesLaboratorio { get; set; }
        public DbSet<Receta> Recetas { get; set; }
        public DbSet<DetalleReceta> DetallesReceta { get; set; }
        public DbSet<Cobro> Cobros { get; set; }
        public DbSet<DetalleCobro> DetallesCobro { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<CuotaMensual> CuotasMensuales { get; set; }
        public DbSet<Donacion> Donaciones { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<CobroVisita> CobrosVisitas { get; set; }
        public DbSet<PagoFundacion> PagosFundacion { get; set; }
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

            modelBuilder.Entity<ExamenLaboratorio>()
                .HasOne(e => e.Paciente)
                .WithMany(p => p.ExamenesLaboratorio)
                .HasForeignKey(e => e.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ExamenLaboratorio>()
                .HasOne(e => e.VisitaMedica)
                .WithMany(v => v.ExamenesLaboratorio)
                .HasForeignKey(e => e.VisitaMedicaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Receta>()
                .HasOne(r => r.Paciente)
                .WithMany(p => p.Recetas)
                .HasForeignKey(r => r.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Receta>()
                .HasOne(r => r.VisitaMedica)
                .WithOne(v => v.Receta)
                .HasForeignKey<Receta>(r => r.VisitaMedicaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DetalleReceta>()
                .HasOne(d => d.Receta)
                .WithMany(r => r.Detalles)
                .HasForeignKey(d => d.RecetaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.Paciente)
                .WithMany()
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cobro>()
                .HasOne(c => c.VisitaMedica)
                .WithMany()
                .HasForeignKey(c => c.VisitaMedicaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<DetalleCobro>()
                .HasOne(d => d.Cobro)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.CobroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Pago>()
                .HasOne(p => p.Cobro)
                .WithMany(c => c.Pagos)
                .HasForeignKey(p => p.CobroId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CuotaMensual>()
                .HasOne(c => c.Paciente)
                .WithMany()
                .HasForeignKey(c => c.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CuotaMensual>()
                .HasOne(c => c.Cobro)
                .WithOne()
                .HasForeignKey<CuotaMensual>(c => c.CobroId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<CobroVisita>()
            .HasOne(cv => cv.Cobro)
            .WithMany()
            .HasForeignKey(cv => cv.CobroId)
            .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CobroVisita>()
                .HasOne(cv => cv.VisitaMedica)
                .WithMany()
                .HasForeignKey(cv => cv.VisitaMedicaId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
