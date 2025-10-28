using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.Models;
using Pharmacy.IRepositories;

namespace Pharmacy.Repositories
{
    public class CountryRepository : ICountryRepository
    {
        private readonly AppDbContext _context;

        public CountryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Country>> GetAllAsync()
        {
            return await _context.Countries
                .Include(c => c.Locations)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Country?> GetByIdAsync(int id)
        {
            return await _context.Countries
                .Include(c => c.Locations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Country> CreateAsync(Country country)
        {
            _context.Countries.Add(country);
            await _context.SaveChangesAsync();
            return country;
        }

        public async Task<Country> UpdateAsync(Country country)
        {
            // Safer update: load existing entity, apply changed properties, then save.
            var existing = await _context.Countries.FindAsync(country.Id);
            if (existing == null)
                throw new InvalidOperationException($"Country with id {country.Id} not found.");

            // Apply only allowed/expected properties
            existing.Name = country.Name;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var country = await _context.Countries.FindAsync(id);
            if (country == null)
                return false;

            _context.Countries.Remove(country);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Countries.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Country>> SearchByNameAsync(string name)
        {
            return await _context.Countries
                .Where(c => c.Name.Contains(name))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}