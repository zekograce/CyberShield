using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberShield.API.Models
{
    public class ProtectionPlan
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string CompanyName { get; set; } // اسم الشركة: Techno Crypt

        [Required]
        [MaxLength(100)]
        public string PlanName { get; set; }    // اسم الباقة: Basic, Premium, Enterprise

        [Range(0, 100)]
        public int DiscountPercentage { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal OldPrice { get; set; }    // السعر قبل الخصم

        // خاصية محسوبة تلقائيًا في الـ C# ومش بتتحفظ كعمود منفصل في الـ DB
        [NotMapped]
        public decimal CurrentPrice
        {
            get
            {
                return OldPrice - (OldPrice * DiscountPercentage / 100);
            }
        }

        public double Rating { get; set; }
        public int ReviewsCount { get; set; }

        // الـ Limits اللي حددناها لكل باقة
        public int MaxFilesPerMonth { get; set; }       // عدد الملفات المسموح بفحصها شهريًا
        public bool UnlimitedLinkScanning { get; set; }   // هل فحص الروابط غير محدود؟
        public int MaxDevicesAllowed { get; set; }      // عدد الأجهزة المتاحة للباقة
        public bool HasAdvancedEmailVerification { get; set; } // ميزة التحقق المتقدم من الإيميل
        public bool HasDedicatedAccountManager { get; set; }   // ميزة مدير الحساب المخصص

        // الميزات النصية اللي الفرونت إند هيعرضها بنقاط في التصميم
        public List<string> Features { get; set; } = new List<string>();
    }
}