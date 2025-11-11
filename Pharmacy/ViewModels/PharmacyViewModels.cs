using System.ComponentModel.DataAnnotations;
using Pharmacy.Models;
using Pharmacy.ViewModels;

namespace Pharmacy.ViewModels
{
    // ViewModel for displaying pharmacy information
    public class PharmacyViewModel
    {
        public int Id { get; set; }
        public string PharmacyName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public bool IsActive { get; set; }
        public string? UserId { get; set; }
        public string? AccountNumber { get; set; }
        public int? LocationsId { get; set; }
        public string? LocationName { get; set; }
        public string? UserDisplayName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // ViewModel for creating/updating pharmacy
    public class PharmacyUpdateViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Pharmacy name is required")]
        [StringLength(100, ErrorMessage = "Pharmacy name cannot exceed 100 characters")]
        [Display(Name = "Pharmacy Name")]
        public string PharmacyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters")]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Password cannot exceed 100 characters")]
        [Display(Name = "Password")]
        public string? Password { get; set; }

        [Display(Name = "Latitude")]
        public string? Latitude { get; set; }

        [Display(Name = "Longitude")]
        public string? Longitude { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; } = true;

        [Required(ErrorMessage = "User is required")]
        [Display(Name = "User")]
        public string UserId { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Account number cannot exceed 50 characters")]
        [Display(Name = "Account Number")]
        public string? AccountNumber { get; set; }

        [Display(Name = "Location")]
        public int? LocationsId { get; set; }

        // For dropdown options
        public List<LocationDropdownViewModel>? AvailableLocations { get; set; }
        public List<UserDropdownViewModel>? AvailableUsers { get; set; }
    }

    // ViewModel for location dropdown
    public class LocationDropdownViewModel
    {
        public int Id { get; set; }
        public string DisplayText { get; set; } = string.Empty;
    }

    // ViewModel for user dropdown
    public class UserDropdownViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
    }
}

namespace Pharmacy.Services
{
    // Extension methods for converting between models and view models
    public static class PharmacyMappingExtensions
    {
        // Convert Pharmacies model to PharmacyViewModel
        public static ViewModels.PharmacyViewModel ToViewModel(this Pharmacies pharmacy)
        {
            return new ViewModels.PharmacyViewModel
            {
                Id = pharmacy.Id,
                PharmacyName = pharmacy.PharmacyName,
                UserName = pharmacy.UserName,
                Password = pharmacy.Password,
                Latitude = pharmacy.Latitude,
                Longitude = pharmacy.Longitude,
                IsActive = pharmacy.IsActive,
                UserId = pharmacy.UserId,
                AccountNumber = pharmacy.AccountNumber,
                LocationsId = pharmacy.LocationsId,
                LocationName = pharmacy.Locations?.City + ", " + pharmacy.Locations?.State,
                UserDisplayName = pharmacy.user?.FullName ?? pharmacy.user?.UserName,
                CreatedAt = pharmacy.CreatedAt,
                UpdatedAt = pharmacy.UpdatedAt
            };
        }

        // Convert Pharmacies model to PharmacyUpdateViewModel
        public static ViewModels.PharmacyUpdateViewModel ToUpdateViewModel(this Pharmacies pharmacy)
        {
            return new ViewModels.PharmacyUpdateViewModel
            {
                Id = pharmacy.Id,
                PharmacyName = pharmacy.PharmacyName,
                UserName = pharmacy.UserName,
                Password = pharmacy.Password,
                Latitude = pharmacy.Latitude,
                Longitude = pharmacy.Longitude,
                IsActive = pharmacy.IsActive,
                UserId = pharmacy.UserId ?? string.Empty,
                AccountNumber = pharmacy.AccountNumber,
                LocationsId = pharmacy.LocationsId
            };
        }

        // Convert PharmacyUpdateViewModel to Pharmacies model
        public static Pharmacies ToModel(this ViewModels.PharmacyUpdateViewModel viewModel)
        {
            return new Pharmacies
            {
                Id = viewModel.Id,
                PharmacyName = viewModel.PharmacyName,
                UserName = viewModel.UserName,
                Password = viewModel.Password,
                Latitude = viewModel.Latitude,
                Longitude = viewModel.Longitude,
                IsActive = viewModel.IsActive,
                UserId = viewModel.UserId,
                AccountNumber = viewModel.AccountNumber,
                LocationsId = viewModel.LocationsId
            };
        }

        // Convert Location model to LocationDropdownViewModel
        public static ViewModels.LocationDropdownViewModel ToDropdownViewModel(this Location location)
        {
            return new ViewModels.LocationDropdownViewModel
            {
                Id = location.Id,
                DisplayText = $"{location.City}, {location.State} ({location.Countries?.Name})"
            };
        }

        // Convert Users model to UserDropdownViewModel
        public static ViewModels.UserDropdownViewModel ToDropdownViewModel(this Users user)
        {
            return new ViewModels.UserDropdownViewModel
            {
                Id = user.Id,
                DisplayText = !string.IsNullOrEmpty(user.FullName) 
                    ? $"{user.FullName} ({user.UserName})" 
                    : $"{user.UserName} ({user.Email})"
            };
        }
    }
}