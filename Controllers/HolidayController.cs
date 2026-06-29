using HRMS.Interfaces;
using HRMS.Services;
using HRMS.Models.Holiday;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using HRMS.Data.Entities;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    [Authorize(Roles = "HR")]
    public class HolidayController : Controller
    {

        private readonly ICompanyService _companyService;

        public HolidayController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        #region HolidaySetup
        /// <summary>
        /// Create holiday from the setup popup.
        /// </summary>
        /// <param name="mdel">The view model containing the holiday model to be saved.</param>
        /// <returns>Redirects to Holiday Setup dashboard with msg.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateHoliday(HolidayViewModel model)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(HolidaySetup));

            var userId = int.TryParse(User.FindFirst("UserId")?.Value, out int id) ? id : 0;

            var result = await _companyService.ConfigurePublicHolidaysAsync(model, userId);

            TempData[result ? "SuccessMessage" : "ErrorMessage"] = result
                ? "Holiday saved successfully!"
                : "This holiday date is already registered!";

            return RedirectToAction(nameof(HolidaySetup));           
        }

        /// <summary>
        /// Fetch all holidays and display the Holiday Setup dashboard.
        /// </summary>
        /// <returns>The view for the Holiday Setup dashboard.</returns>
        [HttpGet]
        [Route("HolidaySetup")]
        [Route("Holiday/HolidaySetup")]
        public async Task<IActionResult> HolidaySetup()
        {
            var holidays = await _companyService.GetAllPublicHolidaysAsync();

            var model= new HolidayViewModel
            {
                Holidays = holidays
            };
            return View(model);
        }
        #endregion


    }
}
