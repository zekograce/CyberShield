using CyberShield.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Package> Packages { get; set; }
        public DbSet<PackageFeature> PackageFeatures { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<SecurityNews> SecurityNews { get; set; }
        public DbSet<SecurityTip> SecurityTips { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<UserSubscription>(entity =>
            {
                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Package)
                    .WithMany(p => p.Subscriptions)
                    .HasForeignKey(s => s.PackageId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PackageFeature>(entity =>
            {
                entity.HasOne(f => f.Package)
                    .WithMany(p => p.Features)
                    .HasForeignKey(f => f.PackageId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.HasOne(u => u.CurrentPackage)
                    .WithMany()
                    .HasForeignKey(u => u.CurrentPackageId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
