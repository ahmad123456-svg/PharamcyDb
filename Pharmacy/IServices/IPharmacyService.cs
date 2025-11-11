using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.IServices
{
    public interface IPharmacyService
    {
        Task<ServiceResult<IEnumerable<Pharmacies>>> GetAllPharmaciesAsync();
        Task<ServiceResult<Pharmacies>> GetPharmacyByIdAsync(int id);
        Task<ServiceResult<Pharmacies>> CreatePharmacyAsync(Pharmacies pharmacy);
        Task<ServiceResult<Pharmacies>> UpdatePharmacyAsync(int id, Pharmacies pharmacy);
        Task<ServiceResult<bool>> DeletePharmacyAsync(int id);
        Task<ServiceResult<bool>> PharmacyExistsAsync(int id);
        Task<ServiceResult<bool>> PharmacyNameExistsAsync(string pharmacyName, int? excludeId = null);
    }
}