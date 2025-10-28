using Pharmacy.Models;

namespace Pharmacy.IRepositories
{
    public interface ILocationRepository
    {
        Task<IEnumerable<Location>> GetAllAsync();
        Task<Location?> GetByIdAsync(int id);
        Task<Location> CreateAsync(Location location);
        Task<Location> UpdateAsync(Location location);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Location>> GetByCountryIdAsync(int countryId);
        Task<IEnumerable<Location>> SearchByCityAsync(string city);
    }
}