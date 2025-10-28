using System.ComponentModel.DataAnnotations;

namespace Pharmacy.ViewModels
{
    public class LocationViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [Display(Name = "Country")]
        public string CountryName { get; set; } = string.Empty;

        public int CountriesId { get; set; }

        [Display(Name = "Time Zone")]
        public string? TimeZone { get; set; }

        [Display(Name = "Created Date")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Full Address")]
        public string FullAddress => $"{Street}, {City}" + 
                                   (string.IsNullOrEmpty(State) ? "" : $", {State}") + 
                                   $", {CountryName}";
    }

    public class LocationCreateViewModel
    {
        [Required(ErrorMessage = "Street address is required")]
        [StringLength(200, ErrorMessage = "Street address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int CountriesId { get; set; }

        [StringLength(50, ErrorMessage = "Time zone cannot exceed 50 characters")]
        [Display(Name = "Time Zone")]
        public string? TimeZone { get; set; }

        // For dropdown
        public IEnumerable<CountryDropdownViewModel> AvailableCountries { get; set; } = new List<CountryDropdownViewModel>();
    }

    public class LocationUpdateViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        [StringLength(200, ErrorMessage = "Street address cannot exceed 200 characters")]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City cannot exceed 100 characters")]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "State cannot exceed 100 characters")]
        [Display(Name = "State/Province")]
        public string? State { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [Display(Name = "Country")]
        public int CountriesId { get; set; }

        [StringLength(50, ErrorMessage = "Time zone cannot exceed 50 characters")]
        [Display(Name = "Time Zone")]
        public string? TimeZone { get; set; }

        // For dropdown
        public IEnumerable<CountryDropdownViewModel> AvailableCountries { get; set; } = new List<CountryDropdownViewModel>();
    }

    public class LocationListViewModel
    {
        public IEnumerable<LocationViewModel> Locations { get; set; } = new List<LocationViewModel>();
        public string? SearchTerm { get; set; }
        public int? SelectedCountryId { get; set; }
        public IEnumerable<CountryDropdownViewModel> AvailableCountries { get; set; } = new List<CountryDropdownViewModel>();
        public int TotalCount { get; set; }
    }

    public class CountryDropdownViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string DisplayText => string.IsNullOrEmpty(Code) ? Name : $"{Name} ({Code})";
    }
}