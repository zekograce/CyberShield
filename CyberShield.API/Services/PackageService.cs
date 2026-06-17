using CyberShield.API.Data;
using CyberShield.API.DTOs;
using CyberShield.API.Models;
using Microsoft.EntityFrameworkCore;

namespace CyberShield.API.Services
{
    public class PackageService : IPackageService
    {
        private readonly ApplicationDbContext _db;

        public PackageService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PackageResponseDto>> GetAllAsync(bool includeInactive = false)
        {
            var query = _db.Packages
                .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(p => p.IsActive);

            var packages = await query.OrderBy(p => (double)p.CurrentPrice).ToListAsync();
            return packages.Select(MapToDto).ToList();
        }

        public async Task<PackageResponseDto?> GetByIdAsync(int id)
        {
            var package = await _db.Packages
                .Include(p => p.Features.OrderBy(f => f.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Id == id);

            return package is null ? null : MapToDto(package);
        }

        public async Task<PackageResponseDto> CreateAsync(CreatePackageDto dto)
        {
            var package = new Package
            {
                Name = dto.Name,
                Description = dto.Description,
                CurrentPrice = dto.CurrentPrice,
                OriginalPrice = dto.OriginalPrice,
                Currency = dto.Currency,
                BillingCycle = dto.BillingCycle,
                IsPopular = dto.IsPopular,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _db.Packages.Add(package);
            await _db.SaveChangesAsync();
            return MapToDto(package);
        }

        public async Task<PackageResponseDto?> UpdateAsync(int id, UpdatePackageDto dto)
        {
            var package = await _db.Packages
                .Include(p => p.Features)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (package is null) return null;

            if (dto.Name is not null) package.Name = dto.Name;
            if (dto.Description is not null) package.Description = dto.Description;
            if (dto.CurrentPrice.HasValue) package.CurrentPrice = dto.CurrentPrice.Value;
            if (dto.OriginalPrice.HasValue) package.OriginalPrice = dto.OriginalPrice.Value;
            if (dto.Currency is not null) package.Currency = dto.Currency;
            if (dto.BillingCycle.HasValue) package.BillingCycle = dto.BillingCycle.Value;
            if (dto.IsPopular.HasValue) package.IsPopular = dto.IsPopular.Value;
            if (dto.IsActive.HasValue) package.IsActive = dto.IsActive.Value;

            await _db.SaveChangesAsync();
            return MapToDto(package);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var package = await _db.Packages.FindAsync(id);
            if (package is null) return false;

            package.IsActive = false;
            await _db.SaveChangesAsync();
            return true;
        }

        private static PackageResponseDto MapToDto(Package p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CurrentPrice = p.CurrentPrice,
            OriginalPrice = p.OriginalPrice,
            Currency = p.Currency,
            BillingCycle = p.BillingCycle.ToString(),
            IsPopular = p.IsPopular,
            IsActive = p.IsActive,
            CreatedAt = p.CreatedAt,
            Features = p.Features.Select(f => new PackageFeatureResponseDto
            {
                Id = f.Id,
                FeatureKey = f.FeatureKey,
                Name = f.Name,
                Value = f.Value,
                DisplayOrder = f.DisplayOrder
            }).ToList()
        };
    }
}
