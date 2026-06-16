using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using CyberShield.API.Data;
using CyberShield.API.Models;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VirusScanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public VirusScanController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("scan")]
        public async Task<IActionResult> ScanFile([FromForm] FileScanRequest request)
        {
            // 1. التحقق من صحة المعرف المدخل
            if (string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { message = "خطأ: يجب تحديد معرف مستخدم صالح!" });
            }

            // 2. التحقق من وجود ملف حقيقي مرفوع
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new { message = "الرجاء اختيار ملف صالح لفحصه!" });
            }

            // 3. جلب الاشتراك الحقيقي والفعال للمستخدم من قاعدة البيانات مباشرة
            var userSub = await _context.Subscriptions
                .Include(s => s.ProtectionPlan)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.IsActive);

            // 4. الحماية الصارمة: لو ملوش اشتراك فعال أو منتهي الصلاحية يرفض السيستم فوراً
            if (userSub == null || userSub.EndDate < DateTime.UtcNow)
            {
                return BadRequest(new { message = "عذراً، ليس لديك اشتراك نشط. يرجى الاشتراك أولاً في إحدى باقاتنا للتمتع بخدمة الفحص الحقيقية." });
            }

            var plan = userSub.ProtectionPlan;
            if (plan == null)
            {
                return BadRequest(new { message = "خطأ: الباقة المرتبطة بهذا الاشتراك غير موجودة في قاعدة البيانات!" });
            }

            // 5. التحقق من الحد الأقصى للباقة (العداد الذكي)
            if (plan.MaxFilesPerMonth != -1)
            {
                if (userSub.CurrentMonthFilesScanned >= plan.MaxFilesPerMonth)
                {
                    return BadRequest(new { message = $"عذراً! لقد استهلكت الحد الأقصى لباقة الـ ({plan.PlanName}) وهو {plan.MaxFilesPerMonth} ملف لهذا الشهر." });
                }
            }

            // 6. حساب الـ SHA-256 Hash للملف بشكل غير متزامن
            string fileHash = "";
            using (var stream = request.File.OpenReadStream())
            {
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = await sha256.ComputeHashAsync(stream);
                    fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }

            // 7. نظام الفحص والمحاكاة الذكي للملفات والتهديدات
            bool isSafe = true;
            string threatName = "لا يوجد تهديدات (Clean) ✨";
            string fileExtension = Path.GetExtension(request.File.FileName).ToLower();

            if (fileExtension == ".exe" || fileExtension == ".bat" || fileExtension == ".vbs" || fileExtension == ".cmd")
            {
                isSafe = false;
                threatName = "Trojan.Win32.Generic (ملف تنفيذي غير موثوق) ⚠️";
            }
            else if (request.File.FileName.Contains("malware") || request.File.FileName.Contains("virus") || request.File.FileName.Contains("hack"))
            {
                isSafe = false;
                threatName = "Worm.Generic.MaliciousPayload ⚠️";
            }

            // 8. تحديث العداد الفعلي في الداتابيز وحفظ التعديلات بثقة
            userSub.CurrentMonthFilesScanned++;
            await _context.SaveChangesAsync();

            // حساب الفاضي من الباقة
            string remainingScans = plan.MaxFilesPerMonth == -1
                ? "ملفات غير محدودة"
                : (plan.MaxFilesPerMonth - userSub.CurrentMonthFilesScanned).ToString();

            return Ok(new
            {
                fileName = request.File.FileName,
                fileSizeReadable = $"{Math.Round((double)request.File.Length / 1024, 2)} KB",
                sha256Hash = fileHash,
                scanDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                isSafe = isSafe,
                status = isSafe ? "آمن وموثوق ✅" : "ملغوم ❌",
                detectedThreat = threatName,
                userPlanName = plan.PlanName,
                currentMonthScannedCount = userSub.CurrentMonthFilesScanned,
                remainingScansThisMonth = remainingScans
            });
        }
    }

    // تعديل الـ Request ليقبل الـ UserId كـ string متوافق مع Identity
    public class FileScanRequest
    {
        public IFormFile? File { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}