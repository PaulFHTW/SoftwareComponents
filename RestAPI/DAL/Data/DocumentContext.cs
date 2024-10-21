using Microsoft.EntityFrameworkCore;
using DAL.Entities;

namespace DAL.Data
{
    public sealed class DocumentContext(DbContextOptions<DocumentContext> options) : DbContext(options)
    {
        public DbSet<Document>? TodoItems { get; set; }

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
                
                entity.Property(e => e.Path).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}