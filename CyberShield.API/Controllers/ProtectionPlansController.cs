using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberShield.API.Data;
using CyberShield.API.Models;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProtectionPlansController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // بنحقن الـ DbContext هنا عشان نقدر نكلم قاعدة البيانات
        public ProtectionPlansController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. رابط الـ GET: api/ProtectionPlans
        // الفرونت إند بيكلمه عشان يعرض الـ 3 باقات اللي في الصور
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProtectionPlan>>> GetProtectionPlans()
        {
            var plans = await _context.ProtectionPlans.ToListAsync();
            return Ok(plans); // هيرجع البيانات كاملة بالأسعار المحسوبة والـ Limits والـ Features كـ Array
        }

        // 2. رابط الـ POST: api/ProtectionPlans
        // بنستخدمه إحنا (أو الأدمن) عشان نضيف باقة جديدة بالـ Limits بتاعتها
        [HttpPost]
        public async Task<ActionResult<ProtectionPlan>> CreateProtectionPlan([FromBody] CreateProtectionPlanDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // بننقل البيانات من الـ DTO للموديل الحقيقي بتاع الداتابيز
            var plan = new ProtectionPlan
            {
                CompanyName = dto.CompanyName,
                PlanName = dto.PlanName,
                DiscountPercentage = dto.DiscountPercentage,
                OldPrice = dto.OldPrice,
                Rating = dto.Rating,
                ReviewsCount = dto.ReviewsCount,

                // حفظ الـ Limits في الداتابيز
                MaxFilesPerMonth = dto.MaxFilesPerMonth,
                UnlimitedLinkScanning = dto.UnlimitedLinkScanning,
                MaxDevicesAllowed = dto.MaxDevicesAllowed,
                HasAdvancedEmailVerification = dto.HasAdvancedEmailVerification,
                HasDedicatedAccountManager = dto.HasDedicatedAccountManager,

                Features = dto.Features
            };

            _context.ProtectionPlans.Add(plan);
            await _context.SaveChangesAsync(); // الحفظ الفعلي في قاعدة البيانات

            return Ok(new { message = "تم إضافة باقة الحماية بالـ Limits بنجاح!", data = plan });
        }
    }

    // الـ DTO: كلاس بسيط بنحطه في آخر ملف الكنترولر أو في فولدر منفصل اسمه DTOs
    // وظيفته يستقبل الداتا الجاية من بره المشروع فقط
    public class CreateProtectionPlanDto
    {
        public string CompanyName { get; set; }
        public string PlanName { get; set; }
        public int DiscountPercentage { get; set; }
        public decimal OldPrice { get; set; }
        public double Rating { get; set; }
        public int ReviewsCount { get; set; }

        // الـ Limits المطلوبة للباقة
        public int MaxFilesPerMonth { get; set; }
        public bool UnlimitedLinkScanning { get; set; }
        public int MaxDevicesAllowed { get; set; }
        public bool HasAdvancedEmailVerification { get; set; }
        public bool HasDedicatedAccountManager { get; set; }

        public List<string> Features { get; set; } = new List<string>();
    }
}