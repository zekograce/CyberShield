using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; // ضيف السطر ده

namespace CyberShield.API.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // تم التغيير إلى string ليتوافق مع الـ Identity

        [Required]
        public int ProtectionPlanId { get; set; }

        [ForeignKey("ProtectionPlanId")]
        public ProtectionPlan? ProtectionPlan { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public int CurrentMonthFilesScanned { get; set; } = 0;
    }
}