using Pharmacy.Models;

namespace Pharmacy.IRepositories
{
    public interface ICountryRepository
    {
        Task<IEnumerable<Country>> GetAllAsync();
        Task<Country?> GetByIdAsync(int id);
        Task<Country> CreateAsync(Country country);
        Task<Country> UpdateAsync(Country country);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<Country>> SearchByNameAsync(string name);
    }
}