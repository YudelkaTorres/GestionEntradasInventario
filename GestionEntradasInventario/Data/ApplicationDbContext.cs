using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestionEntradasInventario.Models;

namespace GestionEntradasInventario.Data;
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
   public DbSet<Productos> Productos { get; set; }
   public DbSet<Entradas> Entradas { get; set; }
   public DbSet<EntradasDetalle> EntradasDetalle { get; set; }

   protected override void OnModelCreating(ModelBuilder builder)
   {
       base.OnModelCreating(builder);

       builder.Entity<EntradasDetalle>()
                .HasOne<Entradas>()       
                .WithMany(e => e.EntradasDetalle)
                .HasForeignKey(d => d.EntradaId)
                .OnDelete(DeleteBehavior.Cascade);

      builder.Entity<EntradasDetalle>()
                .HasOne<Productos>()        
                .WithMany()
                .HasForeignKey(d => d.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
   }
}
