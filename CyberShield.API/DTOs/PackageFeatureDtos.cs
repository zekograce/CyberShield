using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.DTOs
{
    public class CreatePackageFeatureDto
    {
        [Required]
        [MaxLength(100)]
        public string FeatureKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Value { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdatePackageFeatureDto
    {
        [MaxLength(100)]
        public string? FeatureKey { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(200)]
        public string? Value { get; set; }

        public int? DisplayOrder { get; set; }
    }
}
