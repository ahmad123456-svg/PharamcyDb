using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.Models;
using Pharmacy.IRepositories;

namespace Pharmacy.Repositories
{
    public class LocationRepository : ILocationRepository
    {
        private readonly AppDbContext _context;

        public LocationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Location>> GetAllAsync()
        {
            return await _context.Locations
                .Include(l => l.Countries)
                .OrderBy(l => l.City)
                .ToListAsync();
        }

        public async Task<Location?> GetByIdAsync(int id)
        {
            return await _context.Locations
                .Include(l => l.Countries)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Location> CreateAsync(Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
            return location;
        }

        public async Task<Location> UpdateAsync(Location location)
        {
            // Safer update: load existing entity and apply changes to tracked entity
            var existing = await _context.Locations.FindAsync(location.Id);
            if (existing == null)
                throw new InvalidOperationException($"Location with id {location.Id} not found.");

            existing.Street = location.Street;
            existing.City = location.City;
            existing.State = location.State;
            existing.CountriesId = location.CountriesId;
            existing.TimeZone = location.TimeZone;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location == null)
                return false;

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Locations.AnyAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Location>> GetByCountryIdAsync(int countryId)
        {
            return await _context.Locations
                .Include(l => l.Countries)
                .Where(l => l.CountriesId == countryId)
                .OrderBy(l => l.City)
                .ToListAsync();
        }

        public async Task<IEnumerable<Location>> SearchByCityAsync(string city)
        {
            return await _context.Locations
                .Include(l => l.Countries)
                .Where(l => l.City.Contains(city))
                .OrderBy(l => l.City)
                .ToListAsync();
        }
    }
}