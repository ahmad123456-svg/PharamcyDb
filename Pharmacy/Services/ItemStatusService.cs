using Microsoft.Extensions.Logging;
using Pharmacy.IRepositories;
using Pharmacy.Models;
using Pharmacy.IServices;

namespace Pharmacy.Services
{
    public class ItemStatusService : IItemStatusService
    {
        private readonly IItemStatusRepository _itemStatusRepository;
        private readonly ILogger<ItemStatusService> _logger;

        public ItemStatusService(IItemStatusRepository itemStatusRepository, ILogger<ItemStatusService> logger)
        {
            _itemStatusRepository = itemStatusRepository;
            _logger = logger;
        }

        public async Task<ServiceResult<IEnumerable<ItemStatus>>> GetAllItemStatusesAsync()
        {
            try
            {
                var itemStatuses = await _itemStatusRepository.GetAllAsync();
                return ServiceResult<IEnumerable<ItemStatus>>.SuccessResult(itemStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all item statuses");
                return ServiceResult<IEnumerable<ItemStatus>>.ErrorResult("Error retrieving item statuses", 500);
            }
        }

        public async Task<ServiceResult<ItemStatus>> GetItemStatusByIdAsync(int id)
        {
            try
            {
                var itemStatus = await _itemStatusRepository.GetByIdAsync(id);
                if (itemStatus == null)
                {
                    return ServiceResult<ItemStatus>.NotFoundResult("Item status not found");
                }
                return ServiceResult<ItemStatus>.SuccessResult(itemStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item status with id {Id}", id);
                return ServiceResult<ItemStatus>.ErrorResult("Error retrieving item status", 500);
            }
        }

        public async Task<ServiceResult<ItemStatus>> CreateItemStatusAsync(ItemStatus itemStatus)
        {
            try
            {
                var createdItemStatus = await _itemStatusRepository.CreateAsync(itemStatus);
                return ServiceResult<ItemStatus>.SuccessResult(createdItemStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating item status");
                return ServiceResult<ItemStatus>.ErrorResult("Error creating item status", 500);
            }
        }

        public async Task<ServiceResult<ItemStatus>> UpdateItemStatusAsync(int id, ItemStatus itemStatus)
        {
            try
            {
                if (id != itemStatus.Id)
                {
                    return ServiceResult<ItemStatus>.ErrorResult("Invalid item status ID", 400);
                }

                if (!await _itemStatusRepository.ExistsAsync(id))
                {
                    return ServiceResult<ItemStatus>.NotFoundResult("Item status not found");
                }

                var updatedItemStatus = await _itemStatusRepository.UpdateAsync(itemStatus);
                return ServiceResult<ItemStatus>.SuccessResult(updatedItemStatus);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating item status with id {Id}", id);
                return ServiceResult<ItemStatus>.ErrorResult("Error updating item status", 500);
            }
        }

        public async Task<ServiceResult> DeleteItemStatusAsync(int id)
        {
            try
            {
                var result = await _itemStatusRepository.DeleteAsync(id);
                if (!result)
                {
                    return ServiceResult.NotFoundResult("Item status not found");
                }
                return ServiceResult.SuccessResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting item status with id {Id}", id);
                return ServiceResult.ErrorResult("Error deleting item status", 500);
            }
        }

        public async Task<bool> ItemStatusExistsAsync(int id)
        {
            return await _itemStatusRepository.ExistsAsync(id);
        }

        public async Task<ServiceResult<IEnumerable<ItemStatus>>> SearchItemStatusesByNameAsync(string status)
        {
            try
            {
                var itemStatuses = await _itemStatusRepository.SearchByStatusAsync(status);
                return ServiceResult<IEnumerable<ItemStatus>>.SuccessResult(itemStatuses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching item statuses with status {Status}", status);
                return ServiceResult<IEnumerable<ItemStatus>>.ErrorResult("Error searching item statuses", 500);
            }
        }
    }
}