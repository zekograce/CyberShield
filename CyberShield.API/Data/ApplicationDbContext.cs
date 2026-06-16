using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CyberShield.API.Models;

namespace CyberShield.API.Data
{
    // هنا إحنا بنقول للـ Entity Framework إننا هنستخدم ApplicationUser كاليوزر الأساسي للسيستم
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ProtectionPlan> ProtectionPlans { get; set; }
        public DbSet<SecurityNews> SecurityNews { get; set; }
        public DbSet<SecurityTip> SecurityTips { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // هنا ممكن نضيف أي إعدادات خاصة للعلاقات (Relationships) لو احتجنا
        }
    }
}