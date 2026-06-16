using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailVerificationController : ControllerBase
    {
        // موديل لاستقبال البريد الإلكتروني من الفرونت إند
        public class EmailRequest
        {
            public string Email { get; set; } = string.Empty;
        }

        [HttpPost("verify")]
        public IActionResult VerifyEmail([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "الرجاء إدخال بريد إلكتروني صالح!" });
            }

            string email = request.Email.Trim().ToLower();

            // 1. التحقق من الصيغة العامة للإيميل باستخدام الـ Regex
            string emailRegex = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            bool isValidFormat = Regex.IsMatch(email, emailRegex);

            if (!isValidFormat)
            {
                return Ok(new
                {
                    email = email,
                    isValid = false,
                    isDisposable = false,
                    status = "صيغة غير صحيحة ❌",
                    riskLevel = "عالي الخطورة (High)",
                    recommendation = "البريد لا يطابق الصيغة القياسية لرسائل البريد الإلكتروني."
                });
            }

            // 2. قائمة بالنطاقات الوهمية والمؤقتة المشهورة (Disposable Emails)
            var disposableDomains = new List<string>
            {
                "mailinator.com", "yopmail.com", "tempmail.com",
                "10minutemail.com", "sharklasers.com", "guerrillamail.com"
            };

            // استخراج الدومين من الإيميل
            string domain = email.Split('@')[1];
            bool isDisposable = disposableDomains.Contains(domain);

            // 3. تحليل النتيجة وبناء تقرير الأمان
            string status = "آمن وموثوق ✅";
            string riskLevel = "آمن (Safe)";
            string recommendation = "يمكنك التعامل مع هذا البريد الإلكتروني بثقة.";

            if (isDisposable)
            {
                status = "بريد مؤقت / وهمي ⚠️";
                riskLevel = "متوسط الخطورة (Medium)";
                recommendation = "حذر: هذا البريد مؤقت ويُستخدم غالباً للحسابات المزيفة أو الاحتيال.";
            }

            return Ok(new
            {
                email = email,
                domain = domain,
                isValid = true,
                isDisposable = isDisposable,
                status = status,
                riskLevel = riskLevel,
                recommendation = recommendation
            });
        }
    }
}