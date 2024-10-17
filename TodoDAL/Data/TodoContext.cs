using Microsoft.EntityFrameworkCore;
using TodoDAL.Entities;

namespace TodoDAL.Data
{
    public sealed class TodoContext(DbContextOptions<TodoContext> options) : DbContext(options)
    {
        public DbSet<TodoItem>? TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Manuelle Konfiguration der Tabelle
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.ToTable("TodoItems");  // Setzt den Tabellennamen

                entity.HasKey(e => e.Id);  // Setzt den Primärschlüssel

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);  // Konfiguriert den "Name"-Spalten

                entity.Property(e => e.IsComplete);  // Konfiguriert die "IsComplete"-Spalte
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}