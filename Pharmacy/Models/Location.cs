using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy.Models
{
    public class Location
    {
        private DateTime createdAt = DateTime.UtcNow;

        [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Street { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string City { get; set; } = string.Empty;

    [StringLength(100)]
    public string? State { get; set; }

    [Required]
    public int CountriesId { get; set; }

    // Store the user's local date and time when a record is created
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CountriesId")]
        public virtual Country Countries { get; set; } = null!;
        
        [StringLength(50)]
        public string? TimeZone { get; set; }

    
        // Navigation property for pharmacies (uncomment when Pharmacy model is created)
        // public virtual ICollection<Pharmacy> Pharmacies { get; set; } = new List<Pharmacy>();
    }
}