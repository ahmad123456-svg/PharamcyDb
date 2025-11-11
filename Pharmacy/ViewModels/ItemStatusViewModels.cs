using System.ComponentModel.DataAnnotations;

namespace Pharmacy.ViewModels
{
    public class ItemStatusViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        [StringLength(100)]
        public string Status { get; set; } = string.Empty;
    }

    public class ItemStatusCreateViewModel
    {
        [Required]
        [Display(Name = "Status")]
        [StringLength(100)]
        public string Status { get; set; } = string.Empty;
    }

    public class ItemStatusUpdateViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Status")]
        [StringLength(100)]
        public string Status { get; set; } = string.Empty;
    }
}