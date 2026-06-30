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
        public async Task<IActionResult> DailyCheckIn([FromForm] DailyCheckInViewModel model)
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

        #region Attendance History
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
        #endregion

        #region Attendanc Record
        /// <summary>
        /// Retrieves and displays the paginated master attendance records for administrative filtering.
        /// </summary>
        /// <param name="month">The optional month parameter used to filter attendance logs.</param>
        /// <param name="year">The optional year parameter used to filter attendance logs.</param>
        /// <param name="dept">The department name constraint used to isolate records by division.</param>
        /// <param name="employee">The employee identifier sequence or string used for personal identification lookup.</param>
        /// <param name="page">The explicit current page index utilized for list pagination segments, defaulting to 1.</param>
        /// <returns>A web view populated with structural attendance summary view model data segments.</returns>
        public async Task<IActionResult> AttendanceRecord(int? month, int? year, string? dept, string? employee, int page = 1)
        {
            int pageSize = 10;
            var model = await _attendanceService.GetAttendanceRecordsAsync(month, year, dept, employee, page, pageSize);

            return View(model);
        }

        /// <summary>
        /// Generates and exports the isolated employee attendance log records as a downloadable comma-separated spreadsheet document.
        /// </summary>
        /// <param name="month">The active month identifier mapped to restrict data capture boundaries.</param>
        /// <param name="year">The active year identifier mapped to restrict data capture boundaries.</param>
        /// <param name="dept">The filtering criteria to capture records exclusively matching a dedicated department.</param>
        /// <param name="employee">The target employee handle criteria to filter discrete logs before compilation.</param>
        /// <returns>A physical spreadsheet file content container configured with text/csv content streams.</returns>
        [HttpGet]
        public async Task<IActionResult> ExportToExcel(int? month, int? year, string? dept, string? employee)
        {
            var fileBytes = await _attendanceService.ExportAttendanceRecordsAsync(month, year, dept, employee);

            string fileName = $"Attendance_Records_{DateTime.Now:yyyyMMdd}.csv";

            return File(fileBytes, "text/csv", fileName);
        }
        #endregion
    }
}