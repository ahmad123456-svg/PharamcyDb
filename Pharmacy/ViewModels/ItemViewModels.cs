using System.ComponentModel.DataAnnotations;
using Pharmacy.Models;

namespace Pharmacy.ViewModels;

public class ItemCreateViewModel
{
    [Required(ErrorMessage = "Item name is required")]
    [StringLength(100, ErrorMessage = "Item name cannot exceed 100 characters")]
    [Display(Name = "Item Name")]
    public string ItemName { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    [Display(Name = "Item Description")]
    public string? ItemDescription { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    [Display(Name = "Price")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Item status is required")]
    [Display(Name = "Item Status")]
    public int ItemStatusesId { get; set; }

    [StringLength(50, ErrorMessage = "Item code cannot exceed 50 characters")]
    [Display(Name = "Item Code")]
    public string? ItemCode { get; set; }

    [Display(Name = "Expiry Date")]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
    [Display(Name = "Stock")]
    public int? Stock { get; set; }

    [Required(ErrorMessage = "Pharmacy is required")]
    [Display(Name = "Pharmacy")]
    public int PharmaciesId { get; set; }

    // For dropdowns
    public List<ItemStatusDropdownViewModel>? AvailableItemStatuses { get; set; }
    public List<PharmacyDropdownViewModel>? AvailablePharmacies { get; set; }
}

public class ItemUpdateViewModel : ItemCreateViewModel
{
    public int Id { get; set; }
}

public class ItemViewModel
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string? ItemDescription { get; set; }
    public decimal Price { get; set; }
    public string ItemStatusName { get; set; } = string.Empty;
    public string? ItemCode { get; set; }
    public string? InsertedBy { get; set; }
    public DateTime? InsertDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public bool IsActive { get; set; }
    public int? Stock { get; set; }
    public string PharmacyName { get; set; } = string.Empty;
    public int ItemStatusesId { get; set; }
    public int PharmaciesId { get; set; }
}

// ViewModel for item status dropdown
public class ItemStatusDropdownViewModel
{
    public int Id { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

// ViewModel for pharmacy dropdown
public class PharmacyDropdownViewModel
{
    public int Id { get; set; }
    public string DisplayText { get; set; } = string.Empty;
}

public static class ItemMappingExtensions
{
    public static ItemViewModel ToViewModel(this Items item)
    {
        return new ItemViewModel
        {
            Id = item.Id,
            ItemName = item.ItemName,
            ItemDescription = item.ItemDescription,
            Price = item.Price,
            ItemStatusName = item.ItemStatuses?.Status ?? "Unknown",
            ItemCode = item.ItemCode,
            InsertedBy = item.InsertedBy,
            InsertDate = item.InsertDate,
            ExpiryDate = item.ExpiryDate,
            UpdatedBy = item.UpdatedBy,
            UpdateDate = item.UpdateDate,
            IsActive = item.IsActive,
            Stock = item.Stock,
            PharmacyName = item.Pharmacies?.PharmacyName ?? "Unknown",
            ItemStatusesId = item.ItemStatusesId,
            PharmaciesId = item.PharmaciesId
        };
    }

    public static ItemUpdateViewModel ToUpdateViewModel(this Items item)
    {
        return new ItemUpdateViewModel
        {
            Id = item.Id,
            ItemName = item.ItemName,
            ItemDescription = item.ItemDescription,
            Price = item.Price,
            ItemStatusesId = item.ItemStatusesId,
            ItemCode = item.ItemCode,
            ExpiryDate = item.ExpiryDate,
            IsActive = item.IsActive,
            Stock = item.Stock,
            PharmaciesId = item.PharmaciesId
        };
    }

    public static Items ToModel(this ItemCreateViewModel viewModel)
    {
        return new Items
        {
            ItemName = viewModel.ItemName,
            ItemDescription = viewModel.ItemDescription,
            Price = viewModel.Price,
            ItemStatusesId = viewModel.ItemStatusesId,
            ItemCode = viewModel.ItemCode,
            ExpiryDate = viewModel.ExpiryDate,
            IsActive = viewModel.IsActive,
            Stock = viewModel.Stock,
            PharmaciesId = viewModel.PharmaciesId,
            InsertDate = DateTime.Now
        };
    }

    public static void UpdateModel(this Items item, ItemUpdateViewModel viewModel)
    {
        item.ItemName = viewModel.ItemName;
        item.ItemDescription = viewModel.ItemDescription;
        item.Price = viewModel.Price;
        item.ItemStatusesId = viewModel.ItemStatusesId;
        item.ItemCode = viewModel.ItemCode;
        item.ExpiryDate = viewModel.ExpiryDate;
        item.IsActive = viewModel.IsActive;
        item.Stock = viewModel.Stock;
        item.PharmaciesId = viewModel.PharmaciesId;
        item.UpdateDate = DateTime.Now;
    }
}