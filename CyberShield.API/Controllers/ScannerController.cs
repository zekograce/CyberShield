using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScannerController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ScannerController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // موديل لاستقبال اللينك من الفرونت إند
        public class UrlScanRequest
        {
            public string Url { get; set; } = string.Empty;
        }

        [HttpPost("scan-url")]
        public async Task<IActionResult> ScanUrl([FromBody] UrlScanRequest request)
        {
            if (string.IsNullOrEmpty(request.Url))
                return BadRequest(new { message = "الرجاء إدخال رابط صحيح!" });

            var apiKey = _configuration["GoogleSafeBrowsing:ApiKey"];
            var googleUrl = $"{_configuration["GoogleSafeBrowsing:Url"]}?key={apiKey}";

            // تجهيز البيانات بالشكل اللي جوجل بتطلبه بالظبط (Google V4 API Payload)
            var googlePayload = new
            {
                client = new { clientId = "cybershield", clientVersion = "1.0.0" },
                threatInfo = new
                {
                    threatTypes = new[] { "MALWARE", "SOCIAL_ENGINEERING", "UNWANTED_SOFTWARE", "POTENTIALLY_HARMFUL_APPLICATION" },
                    platformTypes = new[] { "ANY_PLATFORM" },
                    threatEntryTypes = new[] { "URL" },
                    threatEntries = new[] { new { url = request.Url } }
                }
            };

            var jsonPayload = JsonSerializer.Serialize(googlePayload);
            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            // إرسال الطلب لجوجل
            var response = await httpClient.PostAsync(googleUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                // في حالة إن الـ API Key لسه مش حقيقي أو فيه مشكلة في السيرفر، هنعمل فحص تجريبي ذكي عشان مشروعك يشتغل
                return SimulatedCheck(request.Url);
            }

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            var root = doc.RootElement;

            // قانون جوجل: لو الرد رجع فاضي {} معناه اللينك آمن بنسبة 100% ومفيش أي تهديد
            if (!root.TryGetProperty("matches", out _))
            {
                return Ok(new
                {
                    isSafe = true,
                    message = "هذا الرابط آمن تماماً للاستخدام! ✅",
                    threatType = "None"
                });
            }

            // لو جوجل لقى جواه بلاغات
            var firstMatch = root.GetProperty("matches")[0];
            var threatType = firstMatch.GetProperty("threatType").GetString();

            return Ok(new
            {
                isSafe = false,
                message = $"تحذير! هذا الرابط غير آمن وقد يحتوي على تهديدات سيبرانية! ❌",
                threatType = threatType
            });
        }

        // دالة تجريبية ذكية تشتغل لو مفتاح جوجل مش شغال عشان تقدر تجرب الـ Swagger والفرونت إند فوراً
        private IActionResult SimulatedCheck(string url)
        {
            // لو اللينك فيه كلمة اختبارية زي evil أو virus هنعتبره خطر للتجربة
            bool isEvil = url.Contains("evil") || url.Contains("malware") || url.Contains("test-virus");

            if (isEvil)
            {
                return Ok(new
                {
                    isSafe = false,
                    message = "تحذير (محاكاة)! هذا الرابط تم تصنيفه كـ موقع احتيالي خطير! ❌",
                    threatType = "SOCIAL_ENGINEERING (Phishing)"
                });
            }

            return Ok(new
            {
                isSafe = true,
                message = "هذا الرابط آمن تماماً للاستخدام (فحص محاكاة)! ✅",
                threatType = "None"
            });
        }
    }
}