using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.IServices
{
    public interface ICountryService
    {
        Task<ServiceResult<IEnumerable<Country>>> GetAllCountriesAsync();
        Task<ServiceResult<Country>> GetCountryByIdAsync(int id);
        Task<ServiceResult<Country>> CreateCountryAsync(Country country);
        Task<ServiceResult<Country>> UpdateCountryAsync(int id, Country country);
        Task<ServiceResult> DeleteCountryAsync(int id);
        Task<bool> CountryExistsAsync(int id);
        Task<ServiceResult<IEnumerable<Country>>> SearchCountriesByNameAsync(string name);
    }
}