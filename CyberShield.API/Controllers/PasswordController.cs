using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PasswordController : ControllerBase
    {
        // موديل لاستقبال الخصائص من شاشة الفرونت إند
        public class PasswordSettings
        {
            public int Length { get; set; } = 12; // الطول الافتراضي للباسورد
            public bool IncludeUppercase { get; set; } = true; // حروف كبيرة
            public bool IncludeLowercase { get; set; } = true; // حروف صغيرة
            public bool IncludeNumbers { get; set; } = true;   // أرقام
            public bool IncludeSpecialChars { get; set; } = true; // رموز خاصة
        }

        [HttpPost("generate")]
        public IActionResult GeneratePassword([FromBody] PasswordSettings settings)
        {
            // التأكد من أن الطول منطقي وآمن
            if (settings.Length < 6 || settings.Length > 64)
            {
                return BadRequest(new { message = "طول كلمة المرور يجب أن يكون بين 6 و 64 حرفاً!" });
            }

            string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string lowercase = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "0123456789";
            string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            StringBuilder charPool = new StringBuilder();
            List<char> guaranteedChars = new List<char>();

            // 1. بناء مخزن الحروف وضمان وجود حرف واحد على الأقل من كل نوع اختاره المستخدم
            if (settings.IncludeUppercase)
            {
                charPool.Append(uppercase);
                guaranteedChars.Add(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
            }
            if (settings.IncludeLowercase)
            {
                charPool.Append(lowercase);
                guaranteedChars.Add(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
            }
            if (settings.IncludeNumbers)
            {
                charPool.Append(numbers);
                guaranteedChars.Add(numbers[RandomNumberGenerator.GetInt32(numbers.Length)]);
            }
            if (settings.IncludeSpecialChars)
            {
                charPool.Append(specialChars);
                guaranteedChars.Add(specialChars[RandomNumberGenerator.GetInt32(specialChars.Length)]);
            }

            // لو المستخدم مقفل كل الاختيارات!
            if (charPool.Length == 0)
            {
                return BadRequest(new { message = "الرجاء اختيار نوع واحد من الحروف على الأقل!" });
            }

            // 2. تملية باقي الطول المطلوب بعشوائية مشفرة وآمنة
            int remainingLength = settings.Length - guaranteedChars.Count;
            for (int i = 0; i < remainingLength; i++)
            {
                int index = RandomNumberGenerator.GetInt32(charPool.Length);
                guaranteedChars.Add(charPool[index]);
            }

            // 3. خلط الحروف الناتجة بشكل عشوائي تماماً عشان الترتيب ميبقاش متوقع (Fisher-Yates Shuffle)
            var finalPassword = guaranteedChars.OrderBy(_ => RandomNumberGenerator.GetInt32(100)).ToArray();

            // 4. تقييم قوة الباسورد بشكل ذكي وسريع
            string strength = "ضعيفة ⚠️";
            if (settings.Length >= 12 && settings.IncludeSpecialChars && settings.IncludeNumbers)
            {
                strength = "قوية جداً حديدية 🔥";
            }
            else if (settings.Length >= 8)
            {
                strength = "متوسطة الأمان 🛡️";
            }

            return Ok(new
            {
                password = new string(finalPassword),
                length = settings.Length,
                strengthText = strength
            });
        }
    }
}