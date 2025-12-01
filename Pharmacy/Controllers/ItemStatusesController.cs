using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.IServices;
using Pharmacy.ViewModels;
using Pharmacy.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharmacy.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ItemStatusesController : Controller
    {
        private readonly IItemStatusService _itemStatusService;
        private readonly ILogger<ItemStatusesController> _logger;

        public ItemStatusesController(IItemStatusService itemStatusService, ILogger<ItemStatusesController> logger)
        {
            _itemStatusService = itemStatusService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetItemStatuses()
        {
            var result = await _itemStatusService.GetAllItemStatusesAsync();
            var itemStatuses = result.Data ?? Enumerable.Empty<Models.ItemStatus>();

            var viewModels = itemStatuses.Select(i => i.ToListViewModel()).ToList();
            return PartialView("_ViewAll", viewModels);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("AddOrEdit", new ItemStatusUpdateViewModel());

            var result = await _itemStatusService.GetItemStatusByIdAsync(id);
            if (!result.Success) return NotFound();

            var vm = new ItemStatusUpdateViewModel
            {
                Id = result.Data!.Id,
                Status = result.Data.Status
            };

            return PartialView("AddOrEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, ItemStatusUpdateViewModel vm)
        {
            try
            {
                // Remove any complex properties from modelstate if present (mirrors example)
                ModelState.Remove("OtherEntities");

                if (ModelState.IsValid)
                {
                    string message;
                    var effectiveId = vm.Id != 0 ? vm.Id : id;

                    if (effectiveId == 0)
                    {
                        // Map viewmodel -> model using MappingService
                        var itemStatusModel = vm.ToModel();
                        _logger.LogDebug("Creating item status: {Status}", itemStatusModel.Status);
                        var createResult = await _itemStatusService.CreateItemStatusAsync(itemStatusModel);
                        if (!createResult.Success)
                        {
                            _logger.LogWarning("CreateItemStatusAsync failed: {Error}", createResult.ErrorMessage);
                            ModelState.AddModelError("", createResult.ErrorMessage ?? "An error occurred while creating the item status.");
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Item status added successfully";
                    }
                    else
                    {
                        // Map viewmodel -> model using MappingService
                        var itemStatusModel = vm.ToModel();
                        itemStatusModel.Id = effectiveId;
                        _logger.LogDebug("Updating item status id={Id} status={Status}", effectiveId, itemStatusModel.Status);
                        var updateResult = await _itemStatusService.UpdateItemStatusAsync(effectiveId, itemStatusModel);
                        if (!updateResult.Success)
                        {
                            _logger.LogWarning("UpdateItemStatusAsync failed for id={Id}: {Error}", effectiveId, updateResult.ErrorMessage);
                            ModelState.AddModelError("", updateResult.ErrorMessage ?? "An error occurred while updating the item status.");
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Item status updated successfully";
                    }

                    var itemStatusesResult = await _itemStatusService.GetAllItemStatusesAsync();
                    var itemStatuses = (itemStatusesResult.Data ?? Enumerable.Empty<Models.ItemStatus>()).Select(i => i.ToListViewModel()).ToList();
                    _logger.LogDebug("Returning item statuses list with {Count} items", itemStatuses.Count);
                    return Json(new
                    {
                        isValid = true,
                        message,
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", itemStatuses)
                    });
                }

                return PartialView("AddOrEdit", vm);
            }
            catch (Exception ex)
            {
                // Log and return a JSON error so client-side can handle it gracefully
                _logger.LogError(ex, "Exception in AddOrEdit");
                return Json(new { isValid = false, message = "An unexpected error occurred: " + ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _itemStatusService.DeleteItemStatusAsync(id);

                if (result.Success)
                {
                    var itemStatusesResult = await _itemStatusService.GetAllItemStatusesAsync();
                    var itemStatuses = (itemStatusesResult.Data ?? Enumerable.Empty<Models.ItemStatus>()).Select(i => i.ToListViewModel()).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Item status deleted successfully",
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", itemStatuses)
                    });
                }
                else
                {
                    return Json(new { success = false, message = result.ErrorMessage });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
