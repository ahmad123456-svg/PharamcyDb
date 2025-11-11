using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy.Models
{
    public class Pharmacies
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string PharmacyName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Password { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public bool IsActive { get; set; } = true;

        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50)]
        public string? AccountNumber { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual Users? user { get; set; }

        // Foreign key for Location
        public int? LocationsId { get; set; }

        [ForeignKey("LocationsId")]
        public virtual Location? Locations { get; set; }

        // Audit fields
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}