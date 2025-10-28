using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;
namespace Pharmacy.Models
{
    [Table("countries")]
    public class Country
    {
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;


    // Navigation property
    public virtual ICollection<Location> Locations { get; set; } = new List<Location>();
    }
}