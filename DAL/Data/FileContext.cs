using Microsoft.EntityFrameworkCore;
using DAL.Entities;
using Entities_File = DAL.Entities.File;
using File = DAL.Entities.File;

namespace DAL.Data
{
    public sealed class FileContext(DbContextOptions<FileContext> options) : DbContext(options)
    {
        public DbSet<Entities_File>? TodoItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Entities_File>(entity =>
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