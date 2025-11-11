using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.Models;
using Pharmacy.IRepositories;

namespace Pharmacy.Repositories
{
    public class ItemStatusRepository : IItemStatusRepository
    {
        private readonly AppDbContext _context;

        public ItemStatusRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemStatus>> GetAllAsync()
        {
            return await _context.ItemStatuses
                .OrderBy(c => c.Status)
                .ToListAsync();
        }

        public async Task<ItemStatus?> GetByIdAsync(int id)
        {
            return await _context.ItemStatuses
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<ItemStatus> CreateAsync(ItemStatus itemStatus)
        {
            await _context.ItemStatuses.AddAsync(itemStatus);
            await _context.SaveChangesAsync();
            return itemStatus;
        }

        public async Task<ItemStatus> UpdateAsync(ItemStatus itemStatus)
        {
            _context.ItemStatuses.Update(itemStatus);
            await _context.SaveChangesAsync();
            return itemStatus;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var itemStatus = await _context.ItemStatuses.FindAsync(id);
            if (itemStatus == null)
            {
                return false;
            }

            _context.ItemStatuses.Remove(itemStatus);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.ItemStatuses.AnyAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<ItemStatus>> SearchByStatusAsync(string status)
        {
            return await _context.ItemStatuses
                .Where(c => c.Status.Contains(status))
                .OrderBy(c => c.Status)
                .ToListAsync();
        }
    }
}