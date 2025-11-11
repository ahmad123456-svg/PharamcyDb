using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pharmacy.Models
{
    [Table("item_statuses")]
    public class ItemStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Status { get; set; } = string.Empty;
    }
}