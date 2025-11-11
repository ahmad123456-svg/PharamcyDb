using Pharmacy.IRepositories;
using Pharmacy.IServices;
using Pharmacy.ViewModels;

namespace Pharmacy.Services;

public class ItemService : IItemService
{
    private readonly IItemRepository _itemRepository;

    public ItemService(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }

    public async Task<ServiceResult<IEnumerable<ItemViewModel>>> GetAllItemsAsync()
    {
        try
        {
            var items = await _itemRepository.GetAllAsync();
            return ServiceResult<IEnumerable<ItemViewModel>>.SuccessResult(items);
        }
        catch (Exception ex)
        {
            return ServiceResult<IEnumerable<ItemViewModel>>.ErrorResult($"Error retrieving items: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ItemUpdateViewModel>> GetItemForEditAsync(int id)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
            {
                return ServiceResult<ItemUpdateViewModel>.ErrorResult("Item not found");
            }

            var viewModel = item.ToUpdateViewModel();
            return ServiceResult<ItemUpdateViewModel>.SuccessResult(viewModel);
        }
        catch (Exception ex)
        {
            return ServiceResult<ItemUpdateViewModel>.ErrorResult($"Error retrieving item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<ItemViewModel>> GetItemByIdAsync(int id)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(id);
            if (item == null)
            {
                return ServiceResult<ItemViewModel>.ErrorResult("Item not found");
            }

            var viewModel = item.ToViewModel();
            return ServiceResult<ItemViewModel>.SuccessResult(viewModel);
        }
        catch (Exception ex)
        {
            return ServiceResult<ItemViewModel>.ErrorResult($"Error retrieving item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> CreateItemAsync(ItemCreateViewModel model, string userName)
    {
        try
        {
            // Check if item name already exists
            if (await _itemRepository.ItemNameExistsAsync(model.ItemName))
            {
                return ServiceResult<bool>.ErrorResult("An item with this name already exists");
            }

            var item = model.ToModel();
            item.InsertedBy = userName;
            item.InsertDate = DateTime.Now;

            var result = await _itemRepository.AddAsync(item);
            if (result)
            {
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.ErrorResult("Failed to create item");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Error creating item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> UpdateItemAsync(ItemUpdateViewModel model, string userName)
    {
        try
        {
            var existingItem = await _itemRepository.GetByIdAsync(model.Id);
            if (existingItem == null)
            {
                return ServiceResult<bool>.ErrorResult("Item not found");
            }

            // Check if item name already exists (excluding current item)
            if (await _itemRepository.ItemNameExistsAsync(model.ItemName, model.Id))
            {
                return ServiceResult<bool>.ErrorResult("An item with this name already exists");
            }

            existingItem.UpdateModel(model);
            existingItem.UpdatedBy = userName;

            var result = await _itemRepository.UpdateAsync(existingItem);
            if (result)
            {
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.ErrorResult("Failed to update item");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Error updating item: {ex.Message}");
        }
    }

    public async Task<ServiceResult<bool>> DeleteItemAsync(int id)
    {
        try
        {
            if (!await _itemRepository.ExistsAsync(id))
            {
                return ServiceResult<bool>.ErrorResult("Item not found");
            }

            var result = await _itemRepository.DeleteAsync(id);
            if (result)
            {
                return ServiceResult<bool>.SuccessResult(true);
            }

            return ServiceResult<bool>.ErrorResult("Failed to delete item");
        }
        catch (Exception ex)
        {
            return ServiceResult<bool>.ErrorResult($"Error deleting item: {ex.Message}");
        }
    }
}