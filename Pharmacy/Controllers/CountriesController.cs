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
    [Authorize(Roles = "Admin")]
    public class CountriesController : Controller
    {
        private readonly ICountryService _countryService;
        private readonly ILogger<CountriesController> _logger;

        public CountriesController(ICountryService countryService, ILogger<CountriesController> logger)
        {
            _countryService = countryService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCountries()
        {
            var result = await _countryService.GetAllCountriesAsync();
            var countries = result.Data ?? Enumerable.Empty<Models.Country>();

            var viewModels = countries.Select(c => c.ToListViewModel()).ToList();
            return PartialView("_ViewAll", viewModels);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
                return PartialView("AddOrEdit", new CountryUpdateViewModel());

            var result = await _countryService.GetCountryByIdAsync(id);
            if (!result.Success) return NotFound();

            var vm = new CountryUpdateViewModel
            {
                Id = result.Data!.Id,
                Name = result.Data.Name
            };

            return PartialView("AddOrEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, CountryUpdateViewModel vm)
        {
            try
            {
                // Remove any complex properties from modelstate if present (mirrors example)
                ModelState.Remove("Locations");

                if (ModelState.IsValid)
                {
                    string message;
                    var effectiveId = vm.Id != 0 ? vm.Id : id;

                    if (effectiveId == 0)
                    {
                        // Map viewmodel -> model using MappingService
                        var countryModel = vm.ToModel();
                        _logger.LogDebug("Creating country: {Name}", countryModel.Name);
                        var createResult = await _countryService.CreateCountryAsync(countryModel);
                        if (!createResult.Success)
                        {
                            _logger.LogWarning("CreateCountryAsync failed: {Error}", createResult.ErrorMessage);
                            ModelState.AddModelError("", createResult.ErrorMessage ?? "An error occurred while creating the country.");
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Country added successfully";
                    }
                    else
                    {
                        // Map viewmodel -> model using MappingService
                        var countryModel = vm.ToModel();
                        countryModel.Id = effectiveId;
                        _logger.LogDebug("Updating country id={Id} name={Name}", effectiveId, countryModel.Name);
                        var updateResult = await _countryService.UpdateCountryAsync(effectiveId, countryModel);
                        if (!updateResult.Success)
                        {
                            _logger.LogWarning("UpdateCountryAsync failed for id={Id}: {Error}", effectiveId, updateResult.ErrorMessage);
                            ModelState.AddModelError("", updateResult.ErrorMessage ?? "An error occurred while updating the country.");
                            return PartialView("AddOrEdit", vm);
                        }

                        message = "Country updated successfully";
                    }

                    var countriesResult = await _countryService.GetAllCountriesAsync();
                    var countries = (countriesResult.Data ?? Enumerable.Empty<Models.Country>()).Select(c => c.ToListViewModel()).ToList();
                    _logger.LogDebug("Returning countries list with {Count} items", countries.Count);
                    return Json(new
                    {
                        isValid = true,
                        message,
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", countries)
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
                var result = await _countryService.DeleteCountryAsync(id);

                if (result.Success)
                {
                    var countriesResult = await _countryService.GetAllCountriesAsync();
                    var countries = (countriesResult.Data ?? Enumerable.Empty<Models.Country>()).Select(c => c.ToListViewModel()).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Country deleted successfully",
                        html = await Helper.RenderRazorViewToString(this, "_ViewAll", countries)
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