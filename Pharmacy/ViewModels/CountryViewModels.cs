using System.ComponentModel.DataAnnotations;

namespace Pharmacy.ViewModels
{
    public class CountryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        [Display(Name = "Country Name")]
        public string Name { get; set; } = string.Empty;

        // Store the user's local date and time when a record is created
    }

    public class CountryCreateViewModel
    {
        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        [Display(Name = "Country Name")]
        public string Name { get; set; } = string.Empty;

    }

    public class CountryUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Country name is required")]
        [StringLength(100, ErrorMessage = "Country name cannot exceed 100 characters")]
        [Display(Name = "Country Name")]
        public string Name { get; set; } = string.Empty;

    }

    public class CountryListViewModel
    {
        public IEnumerable<CountryViewModel> Countries { get; set; } = new List<CountryViewModel>();
        public string? SearchTerm { get; set; }
        public int TotalCount { get; set; }
    }
}