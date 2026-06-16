using System.ComponentModel.DataAnnotations;

namespace CyberShield.API.Models
{
    public class PackageFeature
    {
        public int Id { get; set; }

        public int PackageId { get; set; }
        public virtual Package Package { get; set; } = null!;

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
}
