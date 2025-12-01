using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Pharmacy.IServices;
using Pharmacy.ViewModels;

namespace Pharmacy.Controllers;

[Authorize(Roles = "Admin,SuperAdmin")]
public class ItemsController : Controller
{
    private readonly IItemService _itemService;
    private readonly IItemStatusService _itemStatusService;
    private readonly IPharmacyService _pharmacyService;

    public ItemsController(
        IItemService itemService,
        IItemStatusService itemStatusService,
        IPharmacyService pharmacyService)
    {
        _itemService = itemService;
        _itemStatusService = itemStatusService;
        _pharmacyService = pharmacyService;
    }

    public async Task<IActionResult> Index()
    {
        var result = await _itemService.GetAllItemsAsync();
        if (result.Success)
        {
            return View(result.Data);
        }

        TempData["ErrorMessage"] = result.ErrorMessage;
        return View(new List<ItemViewModel>());
    }

    [HttpPost]
    public async Task<IActionResult> GetAllItems()
    {
        var result = await _itemService.GetAllItemsAsync();
        if (result.Success)
        {
            return PartialView("_ViewAll", result.Data);
        }

        return Json(new { success = false, message = result.ErrorMessage });
    }

    [HttpGet]
    public async Task<IActionResult> AddOrEdit(int id = 0)
    {
        if (id == 0)
        {
            // Adding new item
            var model = new ItemUpdateViewModel();
            await PopulateDropdowns(model);
            return PartialView(model);
        }
        else
        {
            // Editing existing item
            var result = await _itemService.GetItemForEditAsync(id);
            if (result.Success)
            {
                await PopulateDropdowns(result.Data);
                return PartialView(result.Data);
            }

            return Json(new { success = false, message = result.ErrorMessage });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrEdit(ItemUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns(model);
            return PartialView(model);
        }

        var userName = User?.Identity?.Name ?? "System";

        if (model.Id == 0)
        {
            // Create new item
            var createModel = new ItemCreateViewModel
            {
                ItemName = model.ItemName,
                ItemDescription = model.ItemDescription,
                Price = model.Price,
                ItemStatusesId = model.ItemStatusesId,
                ItemCode = model.ItemCode,
                ExpiryDate = model.ExpiryDate,
                IsActive = model.IsActive,
                Stock = model.Stock,
                PharmaciesId = model.PharmaciesId
            };

            var result = await _itemService.CreateItemAsync(createModel, userName);
            if (result.Success)
            {
                return Json(new { success = true, message = "Item added successfully!" });
            }

            ModelState.AddModelError("", result.ErrorMessage ?? "Failed to add item");
        }
        else
        {
            // Update existing item
            var result = await _itemService.UpdateItemAsync(model, userName);
            if (result.Success)
            {
                return Json(new { success = true, message = "Item updated successfully!" });
            }

            ModelState.AddModelError("", result.ErrorMessage ?? "Failed to update item");
        }

        await PopulateDropdowns(model);
        return PartialView(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _itemService.DeleteItemAsync(id);
        if (result.Success)
        {
            return Json(new { success = true, message = "Item deleted successfully!" });
        }

        return Json(new { success = false, message = result.ErrorMessage ?? "Failed to delete item" });
    }

    private async Task PopulateDropdowns(ItemUpdateViewModel model)
    {
        // Get item statuses
        var itemStatusResult = await _itemStatusService.GetAllItemStatusesAsync();
        model.AvailableItemStatuses = itemStatusResult.Success 
            ? itemStatusResult.Data.Select(x => new ItemStatusDropdownViewModel 
            { 
                Id = x.Id, 
                DisplayText = x.Status 
            }).ToList()
            : new List<ItemStatusDropdownViewModel>();

        // Get pharmacies
        var pharmacyResult = await _pharmacyService.GetAllPharmaciesAsync();
        model.AvailablePharmacies = pharmacyResult.Success 
            ? pharmacyResult.Data.Select(x => new PharmacyDropdownViewModel 
            { 
                Id = x.Id, 
                DisplayText = x.PharmacyName 
            }).ToList()
            : new List<PharmacyDropdownViewModel>();
    }
}