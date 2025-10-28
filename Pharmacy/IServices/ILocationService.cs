using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.IServices
{
    public interface ILocationService
    {
        Task<ServiceResult<IEnumerable<Location>>> GetAllLocationsAsync();
        Task<ServiceResult<Location>> GetLocationByIdAsync(int id);
        Task<ServiceResult<Location>> CreateLocationAsync(Location location);
        Task<ServiceResult<Location>> UpdateLocationAsync(int id, Location location);
        Task<ServiceResult> DeleteLocationAsync(int id);
        Task<bool> LocationExistsAsync(int id);
        Task<ServiceResult<IEnumerable<Location>>> GetLocationsByCountryIdAsync(int countryId);
        Task<ServiceResult<IEnumerable<Location>>> SearchLocationsByCityAsync(string city);
    }
}