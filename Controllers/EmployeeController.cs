using HRMS.Models.Employee;
using HRMS.Interfaces;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace HRMS.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly IDashboardService _dashboardService;

        public EmployeeController(IEmployeeService employeeService, IDashboardService dashboardService)
        {
            _employeeService = employeeService;
            _dashboardService = dashboardService;
        }
        public IActionResult Index()
        {
            return RedirectToAction("DailyCheckIn", "Attendance");
        }

        #region HRDashboard
        /// <summary>
        /// Fetches and displays data for the HR Dashboard.
        /// </summary>
        /// <returns>The view for the HR Dashboard.</returns>
        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> HRDashboard()
        {
            var model = await _dashboardService.GetHRDashboardDataAsync();
            return View(model);
        }
        #endregion

        #region Profile
        /// <summary>
        /// Fetches and displays the logged-in employee's profile data
        /// </summary>
        /// <returns>The view for the Profile.</returns>
        [HttpGet]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> Profile()
        {
            if (!TryGetLoggedInUserId(out int loggedInUserId))
            {
                return RedirectToAction("Login", "Auth");
            }
            var model = await _employeeService.GetProfileDataAsync(loggedInUserId);
            return View(model);
        }

        /// <summary>
        /// Submit profile update request
        /// </summary>
        /// <param name="request">The view model containing the employee's updated profile data.</param>
        /// <returns>Redirects to the Profile with success/err msg.</returns>
        [HttpPost]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> SubmitProfileRequest(UpdateProfileRequestViewModel request)
        {
            if (!TryGetLoggedInUserId(out int loggedInUserId))
            {
                return RedirectToAction("Login", "Auth");
            }

            request.UserId = loggedInUserId;
            bool isSaved = await _employeeService.SubmitProfileUpdateRequestAsync(request);

            if (!isSaved)
            {
                TempData["ErrorMessage"] = "Please fill out at least one field to update your profile.";
                return RedirectToAction("Profile");
            }

            TempData["SuccessMessage"] = "Your update request has been submitted successfully.";
            return RedirectToAction("Profile");
        }
        #endregion

        private bool TryGetLoggedInUserId(out int userId)
        {
            userId = 0;
            var userIdClaim = User.FindFirst("UserId")?.Value;
            return !string.IsNullOrWhiteSpace(userIdClaim) && int.TryParse(userIdClaim, out userId);
        }

        #region EmployeeDirectory
        /// <summary>
        /// Employee Directory logic
        /// </summary>
        /// <param name="status">The employment status filter.</param>
        /// <param name="department">The department filter.</param>
        /// <param name="page">The current page number for pagination.</param>
        /// <returns>The view for the Employee Directory.</returns>
        [Route("Admin/EmployeeDirectory")]
        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> EmployeeDirectory(string status = "Active", string department = "All", int page = 1)
        {
            int pageSize = 1; 

            var pagedResult = await _employeeService.GetFilteredEmployeesAsync(status, department, page, pageSize);

            ViewBag.CurrentStatus = status;
            ViewBag.SelectedDept = department;
            ViewBag.TotalRecords = pagedResult.TotalRecords;
            ViewBag.CurrentPage = pagedResult.CurrentPage;
            ViewBag.TotalPages = pagedResult.TotalPages;

            ViewBag.Departments = await _employeeService.GetDepartmentNamesAsync();

            var myViewModel = new HRMS.Models.Employee.DirectoryViewModel
            {
                HRDirectoryList = new List<HRMS.Models.Admin.HRDirectoryViewModel>(),

                EmployeeDirectoryList = pagedResult.Items,
            };

            return View(myViewModel);
        }

        /// <summary>
        /// Get Filtered list of employees
        /// </summary>
        /// <param name="status">The employment status filter.</param>
        /// <param name="department">The department filter.</param>
        /// <param name="page">The current page number for pagination.</param>
        /// <returns>The partial view containing the filtered employees.</returns>
        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GetFilteredEmployees(string status = "Active", string department = "All", int page = 1)
        {
            int pageSize = 1;
            var pagedResult = await _employeeService.GetFilteredEmployeesAsync(status, department, page, pageSize);

            return PartialView("_EmployeeTablePartial", pagedResult.Items);
        }

        /// <summary>
        /// Export employees data to excel file
        /// </summary>
        /// <param name="status">The employment status filter.</param>
        /// <param name="department">The department filter.</param>
        /// <returns>An Excel file containing the employees data.</returns>
        [HttpGet]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ExportEmployeeToExcel(string status = "Active", string department = "All")
        {
            byte[] fileContents = await _employeeService.ExportEmployeesToExcelAsync(status, department);

            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Employee_Report_{DateTime.Now:yyyyMMdd}.xlsx"
            );
        }

        /// <summary>
        /// Register employees with excel file
        /// </summary>
        /// <param name="file">The uploaded Excel file containing employee information.</param>
        /// <returns>A JSON object with msg.</returns>
        [HttpPost]
        [IgnoreAntiforgeryToken]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("File is empty");
            try
            {
                var result = await _employeeService.ReadExcelAsync(file);
                var importResult = await _employeeService.ImportEmployeesFromExcel(result.ValidData);
                var allErrors = result.Errors.Concat(importResult.Errors).ToList();

                if (allErrors.Any())
                {
                    var stream = _employeeService.GenerateErrorExcel(allErrors);
                    string fileName = $"ErrorReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }

                return Json(new { success = true, message = "Import completed successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Approve profile update request.
        /// </summary>
        /// <param name="userId">The ID of the employee whose profile update is being approved.</param>
        /// <returns>Redirects to the Employee Directory.</returns>
        [HttpPost]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ApproveProfileUpdate(int userId)
        {
            var adminIdClaim = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(adminIdClaim) || !int.TryParse(adminIdClaim, out int adminId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var success = await _employeeService.ApproveProfileUpdateAsync(userId, adminId);

            return RedirectToAction("EmployeeDirectory");
        }
        #endregion

    }
}