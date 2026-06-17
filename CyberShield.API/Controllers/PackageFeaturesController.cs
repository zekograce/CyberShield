using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Controllers
{
    [Route("api/packages/{packageId}/features")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class PackageFeaturesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public PackageFeaturesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int packageId, [FromBody] CreatePackageFeatureDto dto)
        {
            var packageExists = await _db.Packages.AnyAsync(p => p.Id == packageId);
            if (!packageExists)
                return NotFound(new { message = "Package not found." });

            var feature = new PackageFeature
            {
                PackageId = packageId,
                FeatureKey = dto.FeatureKey,
                Name = dto.Name,
                Value = dto.Value,
                DisplayOrder = dto.DisplayOrder
            };

            _db.PackageFeatures.Add(feature);
            await _db.SaveChangesAsync();

            return CreatedAtAction(null, new PackageFeatureResponseDto
            {
                Id = feature.Id,
                FeatureKey = feature.FeatureKey,
                Name = feature.Name,
                Value = feature.Value,
                DisplayOrder = feature.DisplayOrder
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int packageId, int id, [FromBody] UpdatePackageFeatureDto dto)
        {
            var feature = await _db.PackageFeatures
                .FirstOrDefaultAsync(f => f.Id == id && f.PackageId == packageId);

            if (feature is null)
                return NotFound(new { message = "Feature not found." });

            if (dto.FeatureKey is not null) feature.FeatureKey = dto.FeatureKey;
            if (dto.Name is not null) feature.Name = dto.Name;
            if (dto.Value is not null) feature.Value = dto.Value;
            if (dto.DisplayOrder.HasValue) feature.DisplayOrder = dto.DisplayOrder.Value;

            await _db.SaveChangesAsync();

            return Ok(new PackageFeatureResponseDto
            {
                Id = feature.Id,
                FeatureKey = feature.FeatureKey,
                Name = feature.Name,
                Value = feature.Value,
                DisplayOrder = feature.DisplayOrder
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int packageId, int id)
        {
            var feature = await _db.PackageFeatures
                .FirstOrDefaultAsync(f => f.Id == id && f.PackageId == packageId);

            if (feature is null)
                return NotFound(new { message = "Feature not found." });

            _db.PackageFeatures.Remove(feature);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Feature deleted successfully." });
        }
    }
}
