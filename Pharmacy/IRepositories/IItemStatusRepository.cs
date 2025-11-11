using Pharmacy.Models;

namespace Pharmacy.IRepositories
{
    public interface IItemStatusRepository
    {
        Task<IEnumerable<ItemStatus>> GetAllAsync();
        Task<ItemStatus?> GetByIdAsync(int id);
        Task<ItemStatus> CreateAsync(ItemStatus itemStatus);
        Task<ItemStatus> UpdateAsync(ItemStatus itemStatus);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<IEnumerable<ItemStatus>> SearchByStatusAsync(string status);
    }
}