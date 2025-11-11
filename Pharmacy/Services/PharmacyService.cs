using Pharmacy.IRepositories;
using Pharmacy.IServices;
using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.Services
{
    public class PharmacyService : IPharmacyService
    {
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly ILogger<PharmacyService> _logger;

        public PharmacyService(IPharmacyRepository pharmacyRepository, ILogger<PharmacyService> logger)
        {
            _pharmacyRepository = pharmacyRepository;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<Pharmacies>>> GetAllPharmaciesAsync()
        {
            try
            {
                _logger.LogInformation("Retrieving all pharmacies");
                var result = await _pharmacyRepository.GetAllPharmaciesAsync();
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully retrieved {Count} pharmacies", result.Data?.Count() ?? 0);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve pharmacies: {Error}", result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetAllPharmaciesAsync");
                return ServiceResult<IEnumerable<Pharmacies>>.ErrorResult("An error occurred while retrieving pharmacies.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> GetPharmacyByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Retrieving pharmacy with ID: {PharmacyId}", id);
                
                if (id <= 0)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Invalid pharmacy ID.");
                }

                var result = await _pharmacyRepository.GetPharmacyByIdAsync(id);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully retrieved pharmacy: {PharmacyName}", result.Data?.PharmacyName);
                }
                else
                {
                    _logger.LogWarning("Failed to retrieve pharmacy with ID {PharmacyId}: {Error}", id, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetPharmacyByIdAsync for ID: {PharmacyId}", id);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while retrieving the pharmacy.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> CreatePharmacyAsync(Pharmacies pharmacy)
        {
            try
            {
                _logger.LogInformation("Creating new pharmacy: {PharmacyName}", pharmacy.PharmacyName);

                // Validate input
                if (string.IsNullOrWhiteSpace(pharmacy.PharmacyName))
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Pharmacy name is required.");
                }

                if (string.IsNullOrWhiteSpace(pharmacy.UserName))
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Username is required.");
                }

                var result = await _pharmacyRepository.CreatePharmacyAsync(pharmacy);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully created pharmacy: {PharmacyName} with ID: {PharmacyId}", 
                        result.Data?.PharmacyName, result.Data?.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to create pharmacy {PharmacyName}: {Error}", pharmacy.PharmacyName, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreatePharmacyAsync for: {PharmacyName}", pharmacy.PharmacyName);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while creating the pharmacy.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> UpdatePharmacyAsync(int id, Pharmacies pharmacy)
        {
            try
            {
                _logger.LogInformation("Updating pharmacy with ID: {PharmacyId}", id);

                if (id <= 0)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Invalid pharmacy ID.");
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(pharmacy.PharmacyName))
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Pharmacy name is required.");
                }

                if (string.IsNullOrWhiteSpace(pharmacy.UserName))
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Username is required.");
                }

                var result = await _pharmacyRepository.UpdatePharmacyAsync(id, pharmacy);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully updated pharmacy: {PharmacyName} with ID: {PharmacyId}", 
                        result.Data?.PharmacyName, id);
                }
                else
                {
                    _logger.LogWarning("Failed to update pharmacy with ID {PharmacyId}: {Error}", id, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in UpdatePharmacyAsync for ID: {PharmacyId}", id);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while updating the pharmacy.");
            }
        }

        public async Task<ServiceResult<bool>> DeletePharmacyAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting pharmacy with ID: {PharmacyId}", id);

                if (id <= 0)
                {
                    return ServiceResult<bool>.ErrorResult("Invalid pharmacy ID.");
                }

                var result = await _pharmacyRepository.DeletePharmacyAsync(id);
                
                if (result.Success)
                {
                    _logger.LogInformation("Successfully deleted pharmacy with ID: {PharmacyId}", id);
                }
                else
                {
                    _logger.LogWarning("Failed to delete pharmacy with ID {PharmacyId}: {Error}", id, result.ErrorMessage);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in DeletePharmacyAsync for ID: {PharmacyId}", id);
                return ServiceResult<bool>.ErrorResult("An error occurred while deleting the pharmacy.");
            }
        }

        public async Task<ServiceResult<bool>> PharmacyExistsAsync(int id)
        {
            try
            {
                return await _pharmacyRepository.PharmacyExistsAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PharmacyExistsAsync for ID: {PharmacyId}", id);
                return ServiceResult<bool>.ErrorResult("An error occurred while checking pharmacy existence.");
            }
        }

        public async Task<ServiceResult<bool>> PharmacyNameExistsAsync(string pharmacyName, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pharmacyName))
                {
                    return ServiceResult<bool>.SuccessResult(false);
                }

                return await _pharmacyRepository.PharmacyNameExistsAsync(pharmacyName, excludeId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PharmacyNameExistsAsync for: {PharmacyName}", pharmacyName);
                return ServiceResult<bool>.ErrorResult("An error occurred while checking pharmacy name.");
            }
        }
    }
}
