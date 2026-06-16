using CyberShield.API.Data;
using CyberShield.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CyberShield.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ProfileController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("my-profile")]
        [Authorize] // لازم يكون مسجل دخول
        public async Task<IActionResult> GetMyProfile()
        {
            // بنجيب الـ ID من التوكن بتاع اليوزر
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var user = await _userManager.Users
                .Include(u => u.CurrentPlan) // عشان نجيب بيانات الباقة
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound("User not found");

            return Ok(new
            {
                user.FullName,
                user.Email,
                user.CreatedAt,
                user.FilesScannedCount,
                user.LinksScannedCount,
                PlanName = user.CurrentPlan?.Name ?? "بدون باقة"
            });
        }
    }
}