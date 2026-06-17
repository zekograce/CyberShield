using CyberShield.API.Data;
using CyberShield.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

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
            if (string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { message = "A valid user ID is required." });

            if (request.File == null || request.File.Length == 0)
                return BadRequest(new { message = "Please select a valid file to scan." });

            var userSub = await _context.UserSubscriptions
                .Include(s => s.Package)
                    .ThenInclude(p => p.Features)
                .FirstOrDefaultAsync(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active);

            if (userSub == null || userSub.EndDate < DateTime.UtcNow)
                return BadRequest(new { message = "No active subscription found. Please subscribe to a package first." });

            var package = userSub.Package;
            var maxFilesFeature = package.Features.FirstOrDefault(f => f.FeatureKey == "MAX_FILES_PER_MONTH");
            bool isUnlimited = maxFilesFeature?.Value == "Unlimited";
            int maxFiles = isUnlimited ? int.MaxValue : (int.TryParse(maxFilesFeature?.Value, out var parsed) ? parsed : int.MaxValue);

            if (!isUnlimited && userSub.CurrentMonthFilesScanned >= maxFiles)
                return BadRequest(new { message = $"Monthly scan limit of {maxFiles} files reached for the {package.Name} plan." });

            string fileHash;
            using (var stream = request.File.OpenReadStream())
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = await sha256.ComputeHashAsync(stream);
                fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }

            bool isSafe = true;
            string threatName = "No threats detected";
            var ext = Path.GetExtension(request.File.FileName).ToLower();

            if (ext is ".exe" or ".bat" or ".vbs" or ".cmd")
            {
                isSafe = false;
                threatName = "Trojan.Win32.Generic (untrusted executable)";
            }
            else if (request.File.FileName.Contains("malware") || request.File.FileName.Contains("virus"))
            {
                isSafe = false;
                threatName = "Worm.Generic.MaliciousPayload";
            }

            userSub.CurrentMonthFilesScanned++;
            await _context.SaveChangesAsync();

            string remaining = isUnlimited ? "Unlimited" : (maxFiles - userSub.CurrentMonthFilesScanned).ToString();

            return Ok(new
            {
                fileName = request.File.FileName,
                fileSizeKb = Math.Round((double)request.File.Length / 1024, 2),
                sha256Hash = fileHash,
                scanDate = DateTime.UtcNow,
                isSafe,
                status = isSafe ? "Safe" : "Threat Detected",
                detectedThreat = threatName,
                packageName = package.Name,
                scansUsedThisMonth = userSub.CurrentMonthFilesScanned,
                scansRemainingThisMonth = remaining
            });
        }
    }

    public class FileScanRequest
    {
        public IFormFile? File { get; set; }
        public string UserId { get; set; } = string.Empty;
    }
}
