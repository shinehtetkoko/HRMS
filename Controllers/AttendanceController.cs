using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRMS.Interfaces;
using System.Threading.Tasks;
using HRMS.Models.Attendance;
using HRMS.Models.Admin;
using System;
using System.Linq;

namespace HRMS.Controllers
{
    [Authorize]
    public class AttendanceController : Controller
    {
        private readonly IAttendanceService _attendanceService;

        public AttendanceController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        #region Helper Method for Dynamic User ID
        /// <summary>
        /// Extracted helper method to securely retrieve the logged-in User's ID from Identity Claims.
        /// </summary>
        private int GetCurrentUserId()
        {          
            var userIdClaim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userIdClaim) ? 0 : int.Parse(userIdClaim);
        }
        #endregion

        #region AttendanceActions(Daily Check-In, Check-Out)
        /// <summary>
        /// Displays the Daily Check-In page and determines if the user has already checked in today.
        /// </summary>
        /// <returns>The check-in view page with setup data.</returns>
        [HttpGet]
        public async Task<IActionResult> DailyCheckIn()
        {
            int currentUserId = GetCurrentUserId();
            if (currentUserId == 0) return RedirectToAction("Login", "Auth");

            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            var todayUtc = DateTime.UtcNow.Date;

            var history = await _attendanceService.GetAttendanceHistoryAsync(currentUserId, currentMonth, currentYear);

            bool isAlreadyCheckedIn = history.Any(a => a.Attendance_Date.Date == todayUtc);

            ViewBag.IsAlreadyCheckedIn = isAlreadyCheckedIn;

            return View();
        }

        /// <summary>
        /// Processes the incoming daily check-in request with location details and attachment for remote.
        /// </summary>
        /// <param name="model">The check-in data payload holding locations and attachments details.</param>
        /// <returns>A JSON response indicating success or failure status.</returns>
        [HttpPost]
        public async Task<IActionResult> DailyCheckIn(DailyCheckInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill all required fields." });
            }

            int currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }

            var result = await _attendanceService.ProcessCheckInAsync(
                currentUserId,
                model.WorkLocation,
                model.CheckInMode,
                model.LocationDetails,
                model.Attachment);

            return Json(new { success = result.Success, message = result.Message });
        }

        /// <summary>
        /// Processes the daily check-out request for the currently logged-in user.
        /// </summary>
        /// <returns>A JSON response indicating success or failure status.</returns>
        [HttpPost]
        public async Task<IActionResult> DailyCheckOut()
        {
            int currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
            {
                return Json(new { success = false, message = "Session expired. Please log in again." });
            }

            var result = await _attendanceService.ProcessCheckOutAsync(currentUserId);

            return Json(new { success = result.Success, message = result.Message });
        }
        #endregion

        #region Attendance History and Records
        /// <summary>
        /// Retrieves and displays the attendance history filtered by month.
        /// </summary>
        /// <param name="month">Optional month filter parameter. If null, current month will be used.</param>
        /// <returns>The attendance history view model mapped with database records.</returns>
        [HttpGet]
        public async Task<IActionResult> AttendanceHistory(int? month)
        {
            int currentUserId = GetCurrentUserId();
            if (currentUserId == 0) return RedirectToAction("Login", "Auth");

            int filterMonth = month ?? DateTime.Now.Month; 
            int filterYear = DateTime.Now.Year; 

            var history = await _attendanceService.GetAttendanceHistoryAsync(currentUserId, filterMonth, filterYear);

            var viewModel = new AttendanceHistoryViewModel
            {
                Attendances = history,
                SelectedMonth = filterMonth,
                SelectedYear = filterYear
            };

            return View(viewModel);
        }

        /// <summary>
        /// Standard entry point action to navigate users to the overview Attendance Record management layout view.
        /// </summary>
        /// <returns>The primary Attendance Record layout view.</returns>
        [HttpGet]
        public IActionResult AttendanceRecord()
        {
            return View();
        }
        #endregion
    }
}