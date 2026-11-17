
using Microsoft.EntityFrameworkCore;
using BloodDonorApp.Models;

namespace BloodDonorApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Donor> Donors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Donor>()
                .HasIndex(d => d.Email)
                .IsUnique();

            modelBuilder.Entity<Donor>()
                .HasIndex(d => d.MobileNumber)
                .IsUnique();
        }
    }
}
