using Pharmacy.Models;
using Pharmacy.Services;

namespace Pharmacy.IServices
{
    public interface IItemStatusService
    {
        Task<ServiceResult<IEnumerable<ItemStatus>>> GetAllItemStatusesAsync();
        Task<ServiceResult<ItemStatus>> GetItemStatusByIdAsync(int id);
        Task<ServiceResult<ItemStatus>> CreateItemStatusAsync(ItemStatus itemStatus);
        Task<ServiceResult<ItemStatus>> UpdateItemStatusAsync(int id, ItemStatus itemStatus);
        Task<ServiceResult> DeleteItemStatusAsync(int id);
        Task<bool> ItemStatusExistsAsync(int id);
        Task<ServiceResult<IEnumerable<ItemStatus>>> SearchItemStatusesByNameAsync(string status);
    }
}