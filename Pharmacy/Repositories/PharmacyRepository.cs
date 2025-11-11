using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.IRepositories;
using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.Repositories
{
    public class PharmacyRepository : IPharmacyRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PharmacyRepository> _logger;

        public PharmacyRepository(AppDbContext context, ILogger<PharmacyRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<Pharmacies>>> GetAllPharmaciesAsync()
        {
            try
            {
                var pharmacies = await _context.Pharmacies
                    .Include(p => p.Locations)
                    .Include(p => p.user)
                    .OrderBy(p => p.PharmacyName)
                    .ToListAsync();

                return ServiceResult<IEnumerable<Pharmacies>>.SuccessResult(pharmacies);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving all pharmacies");
                return ServiceResult<IEnumerable<Pharmacies>>.ErrorResult("An error occurred while retrieving pharmacies.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> GetPharmacyByIdAsync(int id)
        {
            try
            {
                var pharmacy = await _context.Pharmacies
                    .Include(p => p.Locations)
                    .Include(p => p.user)
                    .FirstOrDefaultAsync(p => p.Id == id);

                if (pharmacy == null)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Pharmacy not found.");
                }

                return ServiceResult<Pharmacies>.SuccessResult(pharmacy);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving pharmacy with ID: {PharmacyId}", id);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while retrieving the pharmacy.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> CreatePharmacyAsync(Pharmacies pharmacy)
        {
            try
            {
                // Check if pharmacy name already exists
                var nameExists = await PharmacyNameExistsAsync(pharmacy.PharmacyName);
                if (nameExists.Success && nameExists.Data)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("A pharmacy with this name already exists.");
                }

                pharmacy.CreatedAt = DateTime.UtcNow;
                _context.Pharmacies.Add(pharmacy);
                await _context.SaveChangesAsync();

                // Reload with includes
                var createdPharmacy = await GetPharmacyByIdAsync(pharmacy.Id);
                return createdPharmacy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating pharmacy: {PharmacyName}", pharmacy.PharmacyName);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while creating the pharmacy.");
            }
        }

        public async Task<ServiceResult<Pharmacies>> UpdatePharmacyAsync(int id, Pharmacies pharmacy)
        {
            try
            {
                var existingPharmacy = await _context.Pharmacies.FindAsync(id);
                if (existingPharmacy == null)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("Pharmacy not found.");
                }

                // Check if pharmacy name already exists (excluding current pharmacy)
                var nameExists = await PharmacyNameExistsAsync(pharmacy.PharmacyName, id);
                if (nameExists.Success && nameExists.Data)
                {
                    return ServiceResult<Pharmacies>.ErrorResult("A pharmacy with this name already exists.");
                }

                // Update properties
                existingPharmacy.PharmacyName = pharmacy.PharmacyName;
                existingPharmacy.UserName = pharmacy.UserName;
                existingPharmacy.Password = pharmacy.Password;
                existingPharmacy.Latitude = pharmacy.Latitude;
                existingPharmacy.Longitude = pharmacy.Longitude;
                existingPharmacy.IsActive = pharmacy.IsActive;
                existingPharmacy.UserId = pharmacy.UserId;
                existingPharmacy.AccountNumber = pharmacy.AccountNumber;
                existingPharmacy.LocationsId = pharmacy.LocationsId;
                existingPharmacy.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                // Reload with includes
                var updatedPharmacy = await GetPharmacyByIdAsync(id);
                return updatedPharmacy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating pharmacy with ID: {PharmacyId}", id);
                return ServiceResult<Pharmacies>.ErrorResult("An error occurred while updating the pharmacy.");
            }
        }

        public async Task<ServiceResult<bool>> DeletePharmacyAsync(int id)
        {
            try
            {
                var pharmacy = await _context.Pharmacies.FindAsync(id);
                if (pharmacy == null)
                {
                    return ServiceResult<bool>.ErrorResult("Pharmacy not found.");
                }

                _context.Pharmacies.Remove(pharmacy);
                await _context.SaveChangesAsync();

                return ServiceResult<bool>.SuccessResult(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting pharmacy with ID: {PharmacyId}", id);
                return ServiceResult<bool>.ErrorResult("An error occurred while deleting the pharmacy.");
            }
        }

        public async Task<ServiceResult<bool>> PharmacyExistsAsync(int id)
        {
            try
            {
                var exists = await _context.Pharmacies.AnyAsync(p => p.Id == id);
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if pharmacy exists with ID: {PharmacyId}", id);
                return ServiceResult<bool>.ErrorResult("An error occurred while checking pharmacy existence.");
            }
        }

        public async Task<ServiceResult<bool>> PharmacyNameExistsAsync(string pharmacyName, int? excludeId = null)
        {
            try
            {
                var query = _context.Pharmacies.Where(p => p.PharmacyName.ToLower() == pharmacyName.ToLower());
                
                if (excludeId.HasValue)
                {
                    query = query.Where(p => p.Id != excludeId.Value);
                }

                var exists = await query.AnyAsync();
                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking if pharmacy name exists: {PharmacyName}", pharmacyName);
                return ServiceResult<bool>.ErrorResult("An error occurred while checking pharmacy name.");
            }
        }
    }
}