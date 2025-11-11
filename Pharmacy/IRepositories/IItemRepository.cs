using Pharmacy.Models;
using Pharmacy.ViewModels;

namespace Pharmacy.IRepositories;

public interface IItemRepository
{
    Task<IEnumerable<ItemViewModel>> GetAllAsync();
    Task<Items?> GetByIdAsync(int id);
    Task<bool> AddAsync(Items item);
    Task<bool> UpdateAsync(Items item);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    Task<bool> ItemNameExistsAsync(string itemName, int? excludeId = null);
}