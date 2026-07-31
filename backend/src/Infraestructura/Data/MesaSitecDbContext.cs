using Dominio;
using Microsoft.EntityFrameworkCore;

namespace Infraestructura.Data;

public sealed class MesaSitecDbContext(DbContextOptions<MesaSitecDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Solicitud> Solicitudes => Set<Solicitud>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Nombre).IsRequired().HasMaxLength(200);
            e.HasIndex(t => t.Nombre).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(320);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Nombre).IsRequired().HasMaxLength(200);
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.TenantId);
            e.HasOne<Tenant>().WithMany().HasForeignKey(u => u.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Nombre).IsRequired().HasMaxLength(120);
            e.HasIndex(c => c.TenantId);
            e.HasOne<Tenant>().WithMany().HasForeignKey(c => c.TenantId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Solicitud>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Codigo).IsRequired().HasMaxLength(20);
            e.Property(s => s.Titulo).IsRequired().HasMaxLength(120);
            e.Property(s => s.Descripcion).IsRequired().HasMaxLength(4000);
            e.HasIndex(s => s.TenantId);
            e.HasIndex(s => s.Codigo);
            e.HasIndex(s => new { s.TenantId, s.Estado });
            e.HasIndex(s => new { s.TenantId, s.FechaLimiteSla });

            e.HasOne(s => s.Categoria)
                .WithMany()
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Solicitante)
                .WithMany()
                .HasForeignKey(s => s.SolicitanteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(s => s.Agente)
                .WithMany()
                .HasForeignKey(s => s.AgenteId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
