using Pharmacy.Models;
using Pharmacy.ViewModels;

namespace Pharmacy.Services
{
    public static class MappingService
    {
        // Country mappings
        public static CountryViewModel ToViewModel(this Models.Country country)
        {
            return new CountryViewModel
            {
                Id = country.Id,
                Name = country.Name,
            };
        }

        public static Models.Country ToModel(this CountryCreateViewModel viewModel)
        {
            return new Models.Country
            {
                Name = viewModel.Name
            };
        }

        public static Models.Country ToModel(this CountryUpdateViewModel viewModel)
        {
            return new Models.Country
            {
                Id = viewModel.Id,
                Name = viewModel.Name
            };
        }

        public static CountryDropdownViewModel ToDropdownViewModel(this Models.Country country)
        {
            return new CountryDropdownViewModel
            {
                Id = country.Id,
                Name = country.Name
            };
        }

        public static CountryViewModel ToListViewModel(this Models.Country country)
        {
            return new CountryViewModel
            {
                Id = country.Id,
                Name = country.Name
            };
        }

        // Location mappings
        public static LocationViewModel ToViewModel(this Models.Location location)
        {
            return new LocationViewModel
            {
                Id = location.Id,
                Street = location.Street,
                City = location.City,
                State = location.State,
                CountriesId = location.CountriesId,
                CountryName = location.Countries?.Name ?? "Unknown",
                TimeZone = location.TimeZone,
                CreatedAt = location.CreatedAt ?? DateTime.MinValue
            };
        }

        public static Models.Location ToModel(this LocationCreateViewModel viewModel)
        {
            return new Models.Location
            {
                Street = viewModel.Street,
                City = viewModel.City,
                State = viewModel.State,
                CountriesId = viewModel.CountriesId,
                TimeZone = viewModel.TimeZone,
                CreatedAt = DateTime.UtcNow
            };
        }

        public static Models.Location ToModel(this LocationUpdateViewModel viewModel)
        {
            return new Models.Location
            {
                Id = viewModel.Id,
                Street = viewModel.Street,
                City = viewModel.City,
                State = viewModel.State,
                CountriesId = viewModel.CountriesId,
                TimeZone = viewModel.TimeZone
            };
        }
    }
}