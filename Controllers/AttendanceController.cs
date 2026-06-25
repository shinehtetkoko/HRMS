using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using HRMS.Interfaces;
using System.Threading.Tasks;
using HRMS.Models.Attendance;
using HRMS.Models.Admin;
using System;

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

        [HttpGet]
        public async Task<IActionResult> DailyCheckIn()
        {
            int currentUserId = 1; // Test User ID
            int currentMonth = DateTime.Now.Month;
            int currentYear = DateTime.Now.Year;
            var todayUtc = DateTime.UtcNow.Date;

            var history = await _attendanceService.GetAttendanceHistoryAsync(currentUserId, currentMonth, currentYear);

            bool isAlreadyCheckedIn = history.Any(a => a.Attendance_Date.Date == todayUtc);

            ViewBag.IsAlreadyCheckedIn = isAlreadyCheckedIn;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DailyCheckIn(DailyCheckInViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Please fill all required fields." });
            }

            int currentUserId = 1; // Test User ID

            var result = await _attendanceService.ProcessCheckInAsync(
                currentUserId,
                model.WorkLocation,
                model.CheckInMode,
                model.LocationDetails,
                model.Attachment);

            return Json(new { success = result.Success, message = result.Message });
        }

        public async Task<IActionResult> AttendanceHistory(int? month)
        {
            //var userIdClaim = User.FindFirst("UserId")?.Value;
            //int currentUserId = int.Parse(userIdClaim ?? "1");

            int currentUserId = 1; // Test User ID 

            int filterMonth = month ?? DateTime.Now.Month; //Current Month
            int filterYear = DateTime.Now.Year; // Current Year

            var history = await _attendanceService.GetAttendanceHistoryAsync(currentUserId, filterMonth, filterYear);

            var viewModel = new AttendanceHistoryViewModel
            {
                Attendances = history,
                SelectedMonth = filterMonth,
                SelectedYear = filterYear
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> DailyCheckOut()
        {
            int currentUserId = 1; // Test User ID 

            var result = await _attendanceService.ProcessCheckOutAsync(currentUserId);

            return Json(new { success = result.Success, message = result.Message });
        }

        public IActionResult AttendanceRecord()
        {
            return View();
        }
    }
}