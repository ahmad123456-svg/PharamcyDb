using Pharmacy.Models;
using Pharmacy.IRepositories;
using Pharmacy.IServices;

namespace Pharmacy.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<ServiceResult<IEnumerable<Country>>> GetAllCountriesAsync()
        {
            try
            {
                var countries = await _countryRepository.GetAllAsync();
                return ServiceResult<IEnumerable<Country>>.SuccessResult(countries);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Country>>.ErrorResult($"Error retrieving countries: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Country>> GetCountryByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult<Country>.ErrorResult("Invalid country ID");

                var country = await _countryRepository.GetByIdAsync(id);
                if (country == null)
                    return ServiceResult<Country>.NotFoundResult("Country not found");

                return ServiceResult<Country>.SuccessResult(country);
            }
            catch (Exception ex)
            {
                return ServiceResult<Country>.ErrorResult($"Error retrieving country: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Country>> CreateCountryAsync(Country country)
        {
            try
            {
                // Validation logic
                if (country == null)
                    return ServiceResult<Country>.ErrorResult("Country data is required");

                if (string.IsNullOrWhiteSpace(country.Name))
                    return ServiceResult<Country>.ErrorResult("Country name is required");

                if (country.Name.Length > 100)
                    return ServiceResult<Country>.ErrorResult("Country name cannot exceed 100 characters");

                // Check for duplicate name
                var existingCountries = await _countryRepository.SearchByNameAsync(country.Name);
                if (existingCountries.Any(c => c.Name.Equals(country.Name, StringComparison.OrdinalIgnoreCase)))
                    return ServiceResult<Country>.ErrorResult("A country with this name already exists");

                var createdCountry = await _countryRepository.CreateAsync(country);
                return ServiceResult<Country>.SuccessResult(createdCountry, 201);
            }
            catch (Exception ex)
            {
                return ServiceResult<Country>.ErrorResult($"Error creating country: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Country>> UpdateCountryAsync(int id, Country country)
        {
            try
            {
                // Validation logic
                if (country == null)
                    return ServiceResult<Country>.ErrorResult("Country data is required");

                if (id <= 0)
                    return ServiceResult<Country>.ErrorResult("Invalid country ID");

                if (id != country.Id)
                    return ServiceResult<Country>.ErrorResult("ID mismatch between route and body");

                if (string.IsNullOrWhiteSpace(country.Name))
                    return ServiceResult<Country>.ErrorResult("Country name is required");

                if (country.Name.Length > 100)
                    return ServiceResult<Country>.ErrorResult("Country name cannot exceed 100 characters");

                var existingCountry = await _countryRepository.GetByIdAsync(country.Id);
                if (existingCountry == null)
                    return ServiceResult<Country>.NotFoundResult("Country not found");

                // Check for duplicate name (excluding current country)
                var duplicateCountries = await _countryRepository.SearchByNameAsync(country.Name);
                if (duplicateCountries.Any(c => c.Name.Equals(country.Name, StringComparison.OrdinalIgnoreCase) && c.Id != country.Id))
                    return ServiceResult<Country>.ErrorResult("A country with this name already exists");

                var updatedCountry = await _countryRepository.UpdateAsync(country);
                return ServiceResult<Country>.SuccessResult(updatedCountry);
            }
            catch (Exception ex)
            {
                return ServiceResult<Country>.ErrorResult($"Error updating country: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult> DeleteCountryAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult.ErrorResult("Invalid country ID");

                var country = await _countryRepository.GetByIdAsync(id);
                if (country == null)
                    return ServiceResult.NotFoundResult("Country not found");

                // Check if country has associated locations
                if (country.Locations.Any())
                    return ServiceResult.ErrorResult("Cannot delete country with associated locations", 409);

                var result = await _countryRepository.DeleteAsync(id);
                if (!result)
                    return ServiceResult.ErrorResult("Failed to delete country", 500);

                return ServiceResult.SuccessResult(204);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error deleting country: {ex.Message}", 500);
            }
        }

        public async Task<bool> CountryExistsAsync(int id)
        {
            return await _countryRepository.ExistsAsync(id);
        }

        public async Task<ServiceResult<IEnumerable<Country>>> SearchCountriesByNameAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return await GetAllCountriesAsync();

                var countries = await _countryRepository.SearchByNameAsync(name);
                return ServiceResult<IEnumerable<Country>>.SuccessResult(countries);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Country>>.ErrorResult($"Error searching countries: {ex.Message}", 500);
            }
        }
    }
}