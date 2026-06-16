using Microsoft.AspNetCore.Identity;

namespace CyberShield.API.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int FilesScannedCount { get; set; } = 0;
        public int LinksScannedCount { get; set; } = 0;

        // ربط بالباقة
        public int? CurrentPlanId { get; set; }
        public virtual ProtectionPlan? CurrentPlan { get; set; }
    }
}