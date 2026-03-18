using AniLog.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AniLog.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AnimeLog> AnimeLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AnimeLog>(entity =>
        {
            // MalId unico: no se puede agregar el mismo anime dos veces
            entity.HasIndex(e => e.MalId).IsUnique();

            // Guardar el enum como string legible en la DB (ej: "Watching" en vez de 0)
            entity.Property(e => e.MyStatus).HasConversion<string>();
        });
    }
}
