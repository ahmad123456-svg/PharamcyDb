using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Pharmacy.IServices;
using Pharmacy.ViewModels;
using Pharmacy.Services;
using Pharmacy.Models;
using Pharmacy.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pharmacy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PharmaciesController : Controller
    {
        private readonly IPharmacyService _pharmacyService;
        private readonly ILocationService _locationService;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;
        private readonly ILogger<PharmaciesController> _logger;

        public PharmaciesController(
            IPharmacyService pharmacyService, 
            ILocationService locationService, 
            UserManager<Users> userManager,
            AppDbContext context,
            ILogger<PharmaciesController> logger)
        {
            _pharmacyService = pharmacyService;
            _locationService = locationService;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetPharmacies()
        {
            var result = await _pharmacyService.GetAllPharmaciesAsync();
            var pharmacies = result.Data ?? Enumerable.Empty<Models.Pharmacies>();

            var viewModels = pharmacies.Select(p => p.ToViewModel()).ToList();
            return PartialView("_ViewAll", viewModels);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            var viewModel = new PharmacyUpdateViewModel();

            if (id != 0)
            {
                var result = await _pharmacyService.GetPharmacyByIdAsync(id);
                if (!result.Success) return NotFound();

                viewModel = result.Data!.ToUpdateViewModel();
            }
            else
            {
                // For new pharmacy, set current user as default
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser != null)
                {
                    viewModel.UserId = currentUser.Id;
                }
            }

            // Load locations for dropdown
            var locationsResult = await _locationService.GetAllLocationsAsync();
            viewModel.AvailableLocations = locationsResult.Data?.Select(l => l.ToDropdownViewModel()).ToList() ?? new List<LocationDropdownViewModel>();

            // Load users for dropdown
            var users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
            viewModel.AvailableUsers = users.Select(u => u.ToDropdownViewModel()).ToList();

            return PartialView("AddOrEdit", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, PharmacyUpdateViewModel vm)
        {
            try
            {
                // Remove any complex properties from modelstate if present
                ModelState.Remove("AvailableLocations");
                ModelState.Remove("AvailableUsers");

                if (ModelState.IsValid)
                {
                    string message;
                    var effectiveId = vm.Id != 0 ? vm.Id : id;

                    if (effectiveId == 0)
                    {
                        // Map viewmodel -> model using MappingService
                        var pharmacyModel = vm.ToModel();
                        _logger.LogDebug("Creating pharmacy: {PharmacyName}", pharmacyModel.PharmacyName);
                        var createResult = await _pharmacyService.CreatePharmacyAsync(pharmacyModel);
                        if (!createResult.Success)
                        {
                            _logger.LogWarning("CreatePharmacyAsync failed: {Error}", createResult.ErrorMessage);
                            ModelState.AddModelError("", createResult.ErrorMessage ?? "An error occurred while creating the pharmacy.");
                            
                            // Reload dropdowns
                            var locationsResult = await _locationService.GetAllLocationsAsync();
                            vm.AvailableLocations = locationsResult.Data?.Select(l => l.ToDropdownViewModel()).ToList() ?? new List<LocationDropdownViewModel>();
                            
                            var users = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
                            vm.AvailableUsers = users.Select(u => u.ToDropdownViewModel()).ToList();
                            
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Pharmacy added successfully";
                    }
                    else
                    {
                        // Map viewmodel -> model using MappingService
                        var pharmacyModel = vm.ToModel();
                        pharmacyModel.Id = effectiveId;
                        _logger.LogDebug("Updating pharmacy id={Id} name={PharmacyName}", effectiveId, pharmacyModel.PharmacyName);
                        var updateResult = await _pharmacyService.UpdatePharmacyAsync(effectiveId, pharmacyModel);
                        if (!updateResult.Success)
                        {
                            _logger.LogWarning("UpdatePharmacyAsync failed for id={Id}: {Error}", effectiveId, updateResult.ErrorMessage);
                            ModelState.AddModelError("", updateResult.ErrorMessage ?? "An error occurred while updating the pharmacy.");
                            
                            // Reload dropdowns
                            var locationsResult = await _locationService.GetAllLocationsAsync();
                            vm.AvailableLocations = locationsResult.Data?.Select(l => l.ToDropdownViewModel()).ToList() ?? new List<LocationDropdownViewModel>();
                            
                            var usersReload = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
                            vm.AvailableUsers = usersReload.Select(u => u.ToDropdownViewModel()).ToList();
                            
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Pharmacy updated successfully";
                    }

                    var pharmaciesResult = await _pharmacyService.GetAllPharmaciesAsync();
                    var pharmacies = (pharmaciesResult.Data ?? Enumerable.Empty<Models.Pharmacies>()).Select(p => p.ToViewModel()).ToList();
                    _logger.LogDebug("Returning pharmacies list with {Count} items", pharmacies.Count);
                    return Json(new
                    {
                        isValid = true,
                        message,
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", pharmacies)
                    });
                }

                // Reload dropdowns for invalid model
                var locationsReload = await _locationService.GetAllLocationsAsync();
                vm.AvailableLocations = locationsReload.Data?.Select(l => l.ToDropdownViewModel()).ToList() ?? new List<LocationDropdownViewModel>();
                
                var usersReload2 = await _context.Users.OrderBy(u => u.FullName).ToListAsync();
                vm.AvailableUsers = usersReload2.Select(u => u.ToDropdownViewModel()).ToList();

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
                var result = await _pharmacyService.DeletePharmacyAsync(id);

                if (result.Success)
                {
                    var pharmaciesResult = await _pharmacyService.GetAllPharmaciesAsync();
                    var pharmacies = (pharmaciesResult.Data ?? Enumerable.Empty<Models.Pharmacies>()).Select(p => p.ToViewModel()).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Pharmacy deleted successfully",
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", pharmacies)
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