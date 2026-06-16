using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CyberShield.API.Data;
using CyberShield.API.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // رابط الاشتراك الفعلي: api/Subscriptions/subscribe
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeDto dto)
        {
            // التحقق من أن المدخلات ليست فارغة
            if (string.IsNullOrEmpty(dto.UserId) || dto.PlanId <= 0)
            {
                return BadRequest(new { message = "خطأ: بيانات المستخدم أو الباقة غير صالحة!" });
            }

            // 1. التأكد أن المستخدم موجود فعلاً في جدول الـ AspNetUsers الحقيقي لمنع الـ Foreign Key Exception
            var userExists = await _context.Users.AnyAsync(u => u.Id == dto.UserId);
            if (!userExists)
            {
                return BadRequest(new { message = $"عذراً، لا يوجد مستخدم مسجل في النظام يحمل المعرّف الحسابي ({dto.UserId})!" });
            }

            // 2. التحقق من وجود الباقة المطلوبة في قاعدة البيانات
            var plan = await _context.ProtectionPlans.FindAsync(dto.PlanId);
            if (plan == null)
            {
                return NotFound(new { message = "عذراً، الباقة المطلوبة غير موجودة!" });
            }

            // 3. تعطيل أي باقة قديمة كانت شغالة لنفس المستخدم (نظام باقة واحدة نشطة فقط)
            var oldSubscriptions = await _context.Subscriptions
                .Where(s => s.UserId == dto.UserId && s.IsActive)
                .ToListAsync();

            foreach (var oldSub in oldSubscriptions)
            {
                oldSub.IsActive = false;
            }

            // 4. إنشاء الاشتراك الجديد وربطه بالـ string UserId
            var subscription = new Subscription
            {
                UserId = dto.UserId,
                ProtectionPlanId = dto.PlanId,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddDays(30), // صلاحية الباقة 30 يوماً
                IsActive = true,
                CurrentMonthFilesScanned = 0
            };

            // الحفظ الفعلي والمباشر في قاعدة البيانات بثقة وأمان
            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"تم الاشتراك في باقة ({plan.PlanName}) بنجاح! الاشتراك الفعلي نشط الآن." });
        }
    }

    // الـ DTO المحدث ليتوافق مع الـ Identity الحالي (UserId أصبح string)
    public class SubscribeDto
    {
        public string UserId { get; set; } = string.Empty;
        public int PlanId { get; set; }
    }
}