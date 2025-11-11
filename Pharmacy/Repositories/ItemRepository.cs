using Microsoft.EntityFrameworkCore;
using Pharmacy.Data;
using Pharmacy.IRepositories;
using Pharmacy.Models;
using Pharmacy.ViewModels;

namespace Pharmacy.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ItemViewModel>> GetAllAsync()
    {
        var items = await _context.Items
            .Include(i => i.ItemStatuses)
            .Include(i => i.Pharmacies)
            .OrderBy(i => i.ItemName)
            .ToListAsync();

        return items.Select(i => i.ToViewModel());
    }

    public async Task<Items?> GetByIdAsync(int id)
    {
        return await _context.Items
            .Include(i => i.ItemStatuses)
            .Include(i => i.Pharmacies)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<bool> AddAsync(Items item)
    {
        try
        {
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> UpdateAsync(Items item)
    {
        try
        {
            _context.Items.Update(item);
            await _context.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                _context.Items.Remove(item);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Items.AnyAsync(i => i.Id == id);
    }

    public async Task<bool> ItemNameExistsAsync(string itemName, int? excludeId = null)
    {
        var query = _context.Items.Where(i => i.ItemName.ToLower() == itemName.ToLower());
        
        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }
        
        return await query.AnyAsync();
    }
}