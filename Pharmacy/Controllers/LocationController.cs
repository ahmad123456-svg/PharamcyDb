using Microsoft.AspNetCore.Mvc;
using Pharmacy.IServices;
using Pharmacy.ViewModels;
using Pharmacy.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pharmacy.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationService _locationService;
        private readonly ICountryService _countryService;
        private readonly ILogger<LocationController> _logger;

        public LocationController(ILocationService locationService, ICountryService countryService, ILogger<LocationController> logger)
        {
            _locationService = locationService;
            _countryService = countryService;
            _logger = logger;
        }

        // GET: Location
        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetLocations(string searchTerm = "")
        {
            var result = await _locationService.GetAllLocationsAsync();
                var locations = result.Data ?? Enumerable.Empty<Models.Location>();

            var viewModels = locations.Select(l => l.ToViewModel()).ToList();
            return PartialView("_ViewAll", viewModels);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            // Get countries for dropdown
            var countriesResult = await _countryService.GetAllCountriesAsync();
            var availableCountries = new List<CountryDropdownViewModel>();
            availableCountries = countriesResult.Data?.Select(c => c.ToDropdownViewModel()).ToList() ?? new List<CountryDropdownViewModel>();

            if (id == 0)
            {
                var viewModel = new LocationCreateViewModel
                {
                    AvailableCountries = availableCountries
                };
                return PartialView("AddOrEdit", viewModel);
            }

            var result = await _locationService.GetLocationByIdAsync(id);
            if (!result.Success) return NotFound();

            var updateViewModel = new LocationUpdateViewModel
            {
                Id = result.Data!.Id,
                Street = result.Data.Street,
                City = result.Data.City,
                State = result.Data.State,
                CountriesId = result.Data.CountriesId,
                TimeZone = result.Data.TimeZone,
                AvailableCountries = availableCountries
            };

            return PartialView("AddOrEdit", updateViewModel);
        }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddOrEdit(int id, LocationUpdateViewModel vm)
        {
            try
            {
        _logger?.LogDebug("Location AddOrEdit called with id={Id} vm.Id={VmId} Street={Street} City={City} CountriesId={CountriesId}", id, vm?.Id, vm?.Street, vm?.City, vm?.CountriesId);
                // Determine effective id (supports forms that post id separately)
                var effectiveId = vm?.Id != 0 ? vm.Id : id;
        _logger?.LogDebug("EffectiveId computed = {EffectiveId}", effectiveId);

                // Create
                if (effectiveId == 0)
                {
                    if (ModelState.IsValid)
                    {
                        var location = vm.ToModel();
                        // Ensure Id is zero for create
                        location.Id = 0;
                        var result = await _locationService.CreateLocationAsync(location);

                        if (result.Success)
                        {
                            var locations = await _locationService.GetAllLocationsAsync();
                            var viewModels = locations.Data?.Select(l => l.ToViewModel()).ToList() ?? new List<LocationViewModel>();

                            return Json(new
                            {
                                isValid = true,
                                message = "Location added successfully",
                                html = await Helper.RenderRazorViewToString(this, "_ViewAll", viewModels)
                            });
                        }

                        ModelState.AddModelError("", result.ErrorMessage ?? "An error occurred while creating the location.");
                    }

                    // Validation failed - reload countries and return create-shaped model
                    var countriesResult = await _countryService.GetAllCountriesAsync();
                    var createModel = new LocationCreateViewModel
                    {
                        Street = vm?.Street ?? string.Empty,
                        City = vm?.City ?? string.Empty,
                        State = vm?.State,
                        CountriesId = vm?.CountriesId ?? 0,
                        TimeZone = vm?.TimeZone,
                        AvailableCountries = countriesResult.Data?.Select(c => c.ToDropdownViewModel()).ToList() ?? new List<CountryDropdownViewModel>()
                    };

                    return PartialView("AddOrEdit", createModel);
                }

                // Update
                if (effectiveId != vm.Id)
                    return NotFound();

                if (ModelState.IsValid)
                {
                    var location = vm.ToModel();
                    var result = await _locationService.UpdateLocationAsync(effectiveId, location);

                    if (result.Success)
                    {
                        var locations = await _locationService.GetAllLocationsAsync();
                        var viewModels = locations.Data?.Select(l => l.ToViewModel()).ToList() ?? new List<LocationViewModel>();

                        return Json(new
                        {
                            isValid = true,
                            message = "Location updated successfully",
                            html = await Helper.RenderRazorViewToString(this, "_ViewAll", viewModels)
                        });
                    }

                    ModelState.AddModelError("", result.ErrorMessage ?? "An error occurred while updating the location.");
                }

                // Validation failed - reload countries and return update model
                var countriesReload = await _countryService.GetAllCountriesAsync();
                vm.AvailableCountries = countriesReload.Data?.Select(c => c.ToDropdownViewModel()).ToList() ?? new List<CountryDropdownViewModel>();

                return PartialView("AddOrEdit", vm);
            }
            catch (Exception ex)
            {
                // Return JSON error to client
                return Json(new { isValid = false, message = "An unexpected error occurred: " + ex.Message });
            }
    }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _locationService.DeleteLocationAsync(id);
                
                if (result.Success)
                {
                    var locations = await _locationService.GetAllLocationsAsync();
                    var viewModels = locations.Data?.Select(l => l.ToViewModel()).ToList() ?? new List<LocationViewModel>();
                    
                    return Json(new
                    {
                        success = true,
                        message = "Location deleted successfully",
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", viewModels)
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