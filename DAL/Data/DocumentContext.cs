using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Data;

public sealed class DocumentContext(DbContextOptions<DocumentContext> options) : DbContext(options)
{
    public DbSet<Document>? Documents { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Document>(entity =>
        {
            entity.ToTable("Files"); 

            entity.HasKey(e => e.Id); 

            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.UploadDate)
                .IsRequired()
                .HasColumnType("timestamp");
        });

        base.OnModelCreating(modelBuilder);
    }
}