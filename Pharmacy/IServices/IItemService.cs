using Pharmacy.Services;
using Pharmacy.ViewModels;

namespace Pharmacy.IServices;

public interface IItemService
{
    Task<ServiceResult<IEnumerable<ItemViewModel>>> GetAllItemsAsync();
    Task<ServiceResult<ItemUpdateViewModel>> GetItemForEditAsync(int id);
    Task<ServiceResult<ItemViewModel>> GetItemByIdAsync(int id);
    Task<ServiceResult<bool>> CreateItemAsync(ItemCreateViewModel model, string userName);
    Task<ServiceResult<bool>> UpdateItemAsync(ItemUpdateViewModel model, string userName);
    Task<ServiceResult<bool>> DeleteItemAsync(int id);
}