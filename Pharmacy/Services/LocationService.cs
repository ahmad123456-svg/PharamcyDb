using Pharmacy.Models;
using Pharmacy.IRepositories;
using Pharmacy.IServices;

namespace Pharmacy.Services
{
    public class LocationService : ILocationService
    {
        private readonly ILocationRepository _locationRepository;
        private readonly ICountryRepository _countryRepository;

        public LocationService(ILocationRepository locationRepository, ICountryRepository countryRepository)
        {
            _locationRepository = locationRepository;
            _countryRepository = countryRepository;
        }

        public async Task<ServiceResult<IEnumerable<Location>>> GetAllLocationsAsync()
        {
            try
            {
                var locations = await _locationRepository.GetAllAsync();
                return ServiceResult<IEnumerable<Location>>.SuccessResult(locations);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Location>>.ErrorResult($"Error retrieving locations: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Location>> GetLocationByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult<Location>.ErrorResult("Invalid location ID");

                var location = await _locationRepository.GetByIdAsync(id);
                if (location == null)
                    return ServiceResult<Location>.NotFoundResult("Location not found");

                return ServiceResult<Location>.SuccessResult(location);
            }
            catch (Exception ex)
            {
                return ServiceResult<Location>.ErrorResult($"Error retrieving location: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Location>> CreateLocationAsync(Location location)
        {
            try
            {
                // Validation logic
                if (location == null)
                    return ServiceResult<Location>.ErrorResult("Location data is required");

                if (string.IsNullOrWhiteSpace(location.Street))
                    return ServiceResult<Location>.ErrorResult("Street is required");

                if (string.IsNullOrWhiteSpace(location.City))
                    return ServiceResult<Location>.ErrorResult("City is required");

                if (location.Street.Length > 200)
                    return ServiceResult<Location>.ErrorResult("Street cannot exceed 200 characters");

                if (location.City.Length > 100)
                    return ServiceResult<Location>.ErrorResult("City cannot exceed 100 characters");

                if (!string.IsNullOrWhiteSpace(location.State) && location.State.Length > 100)
                    return ServiceResult<Location>.ErrorResult("State cannot exceed 100 characters");

                if (!string.IsNullOrWhiteSpace(location.TimeZone) && location.TimeZone.Length > 50)
                    return ServiceResult<Location>.ErrorResult("TimeZone cannot exceed 50 characters");

                if (location.CountriesId <= 0)
                    return ServiceResult<Location>.ErrorResult("Valid country is required");

                // Verify country exists
                var countryExists = await _countryRepository.ExistsAsync(location.CountriesId);
                if (!countryExists)
                    return ServiceResult<Location>.ErrorResult("Invalid country specified");

                // Ensure CreatedAt is set (DB column is non-nullable in current schema)
                if (!location.CreatedAt.HasValue)
                {
                    location.CreatedAt = DateTime.UtcNow;
                }

                var createdLocation = await _locationRepository.CreateAsync(location);
                return ServiceResult<Location>.SuccessResult(createdLocation, 201);
            }
            catch (Exception ex)
            {
                return ServiceResult<Location>.ErrorResult($"Error creating location: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<Location>> UpdateLocationAsync(int id, Location location)
        {
            try
            {
                // Validation logic
                if (location == null)
                    return ServiceResult<Location>.ErrorResult("Location data is required");

                if (id <= 0)
                    return ServiceResult<Location>.ErrorResult("Invalid location ID");

                if (id != location.Id)
                    return ServiceResult<Location>.ErrorResult("ID mismatch between route and body");

                if (string.IsNullOrWhiteSpace(location.Street))
                    return ServiceResult<Location>.ErrorResult("Street is required");

                if (string.IsNullOrWhiteSpace(location.City))
                    return ServiceResult<Location>.ErrorResult("City is required");

                if (location.Street.Length > 200)
                    return ServiceResult<Location>.ErrorResult("Street cannot exceed 200 characters");

                if (location.City.Length > 100)
                    return ServiceResult<Location>.ErrorResult("City cannot exceed 100 characters");

                if (!string.IsNullOrWhiteSpace(location.State) && location.State.Length > 100)
                    return ServiceResult<Location>.ErrorResult("State cannot exceed 100 characters");

                if (!string.IsNullOrWhiteSpace(location.TimeZone) && location.TimeZone.Length > 50)
                    return ServiceResult<Location>.ErrorResult("TimeZone cannot exceed 50 characters");

                if (location.CountriesId <= 0)
                    return ServiceResult<Location>.ErrorResult("Valid country is required");

                var existingLocation = await _locationRepository.GetByIdAsync(location.Id);
                if (existingLocation == null)
                    return ServiceResult<Location>.NotFoundResult("Location not found");

                // Verify country exists
                var countryExists = await _countryRepository.ExistsAsync(location.CountriesId);
                if (!countryExists)
                    return ServiceResult<Location>.ErrorResult("Invalid country specified");

                var updatedLocation = await _locationRepository.UpdateAsync(location);
                return ServiceResult<Location>.SuccessResult(updatedLocation);
            }
            catch (Exception ex)
            {
                return ServiceResult<Location>.ErrorResult($"Error updating location: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult> DeleteLocationAsync(int id)
        {
            try
            {
                if (id <= 0)
                    return ServiceResult.ErrorResult("Invalid location ID");

                var location = await _locationRepository.GetByIdAsync(id);
                if (location == null)
                    return ServiceResult.NotFoundResult("Location not found");

                // Add business logic - check if location has associated pharmacies (when Pharmacy model exists)
                // if (location.Pharmacies.Any())
                //     return ServiceResult.ErrorResult("Cannot delete location with associated pharmacies", 409);

                var result = await _locationRepository.DeleteAsync(id);
                if (!result)
                    return ServiceResult.ErrorResult("Failed to delete location", 500);

                return ServiceResult.SuccessResult(204);
            }
            catch (Exception ex)
            {
                return ServiceResult.ErrorResult($"Error deleting location: {ex.Message}", 500);
            }
        }

        public async Task<bool> LocationExistsAsync(int id)
        {
            return await _locationRepository.ExistsAsync(id);
        }

        public async Task<ServiceResult<IEnumerable<Location>>> GetLocationsByCountryIdAsync(int countryId)
        {
            try
            {
                if (countryId <= 0)
                    return ServiceResult<IEnumerable<Location>>.ErrorResult("Invalid country ID");

                // Verify country exists
                var countryExists = await _countryRepository.ExistsAsync(countryId);
                if (!countryExists)
                    return ServiceResult<IEnumerable<Location>>.ErrorResult("Country not found", 404);

                var locations = await _locationRepository.GetByCountryIdAsync(countryId);
                return ServiceResult<IEnumerable<Location>>.SuccessResult(locations);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Location>>.ErrorResult($"Error retrieving locations by country: {ex.Message}", 500);
            }
        }

        public async Task<ServiceResult<IEnumerable<Location>>> SearchLocationsByCityAsync(string city)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(city))
                    return await GetAllLocationsAsync();

                var locations = await _locationRepository.SearchByCityAsync(city);
                return ServiceResult<IEnumerable<Location>>.SuccessResult(locations);
            }
            catch (Exception ex)
            {
                return ServiceResult<IEnumerable<Location>>.ErrorResult($"Error searching locations: {ex.Message}", 500);
            }
        }
    }
}