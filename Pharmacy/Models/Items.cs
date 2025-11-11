using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy.Models;

public class Items
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? ItemDescription { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Price { get; set; }

    [Required]
    public int ItemStatusesId { get; set; }

    [StringLength(50)]
    public string? ItemCode { get; set; }

    public string? InsertedBy { get; set; }

    public DateTime? InsertDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public string? UpdatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public bool IsActive { get; set; } = true;

    public int? Stock { get; set; }

    [Required]
    public int PharmaciesId { get; set; }

    // Navigation Properties
    public virtual ItemStatus? ItemStatuses { get; set; }
    public virtual Pharmacies? Pharmacies { get; set; }
}