using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pharmacy.Models;

namespace Pharmacy.Data
{
    public class AppDbContext:IdentityDbContext<Users>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }
        public DbSet<Location> Locations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Country entity
            modelBuilder.Entity<Country>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.HasIndex(e => e.Name)
                    .IsUnique();
            });

            // Configure Location entity
            modelBuilder.Entity<Location>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Street)
                    .IsRequired()
                    .HasMaxLength(200);
                entity.Property(e => e.City)
                    .IsRequired()
                    .HasMaxLength(100);
                entity.Property(e => e.State)
                    .HasMaxLength(100);
                entity.Property(e => e.TimeZone)
                    .HasMaxLength(50);

                // Configure foreign key relationship
                entity.HasOne(l => l.Countries)
                    .WithMany(c => c.Locations)
                    .HasForeignKey(l => l.CountriesId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
