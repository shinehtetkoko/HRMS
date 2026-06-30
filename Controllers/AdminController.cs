using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HRMS.Interfaces;
using HRMS.Services;
using HRMS.Models.Employee;
using HRMS.Models.Admin;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HRMS.Enums;

namespace HRMS.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;
        private readonly IDashboardService _dashboardService;

        public AdminController(ICompanyService companyService, IEmployeeService employeeService, IDashboardService dashboardService)
        {
            _companyService = companyService;
            _employeeService = employeeService;
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Fetches and displays data for the Admin Dashboard.
        /// </summary>
        /// <returns>The view for the Admin Dashboard.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            var model = await _dashboardService.GetAdminDashboardDataAsync();
            return View(model);
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        #region CompanyProfile
        /// <summary>
        /// Retrieves the official company profile details to display on the profile page.
        /// </summary>
        /// <returns>Returns the CompanyProfileViewModel, or a new empty ViewModel if no record exists.</returns>
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CompanyProfile()
        {
            var companyViewModel = await _companyService.GetCompanyProfileAsync();
            return View(companyViewModel ?? new CompanyProfileViewModel());
        }

        /// <summary>
        /// Updates the company profile details.
        /// </summary>
        /// <param name="model">The company profile data payload containing updated details.</param>
        /// <returns>An OK status with a success message, or an error status if the update fails.</returns>

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateCompanyProfile([FromBody] CompanyProfileViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { message = "Data is null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _companyService.UpdateCompanyProfileAsync(model);

            if (!result.Success)
            {
                if (result.Message.Contains("not found"))
                {
                    return NotFound(new { message = result.Message });
                }
                return StatusCode(500, result.Message);
            }

            return Ok(new { message = result.Message });
        }
        #endregion

        #region HRDirectory
        /// <summary>
        /// Retrieves and displays the list of HR accounts based on their active or resigned status.
        /// </summary>
        /// <param name="status">The account status filter, default is "Active".</param>
        /// <returns>The HR directory view populated with the HR accounts list.</returns>
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> HRDirectory(string status = "Active")
        {
            var hrList = await _employeeService.GetHRDirectoryListAsync(status);

            ViewBag.CurrentStatus = status;

            return View(hrList);
        }

        /// <summary>
        /// Registers a new HR account inside the system and generates an automated password.
        /// </summary>
        /// <param name="model">The registration data payload for the new HR account.</param>
        /// <returns>A 200 OK status on success, or a 400 BadRequest if registration fails.</returns>
        [HttpPost]
        public async Task<IActionResult> RegisterHRAccount([FromBody] UserRegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Data is null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data format" });
            }

            var result = await _employeeService.RegisterNewUserAccountAsync(model);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// Updates the employment status and resignation records of an HR account.
        /// </summary>
        /// <param name="model">The update data payload holding account status change details.</param>
        /// <returns>A 200 OK status on success, or an error status if the operation fails.</returns>
        [HttpPost]
        public async Task<IActionResult> UpdateHRStatus([FromBody] HRUpdateViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Data is null" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data format" });
            }

            var adminIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int currentAdminId = int.Parse(adminIdClaim ?? "1");

            var isSaved = await _employeeService.UpdateHRStatusAsync(model, currentAdminId);

            if (!isSaved)
            {
                return StatusCode(500, new { success = false, message = "Something went wrong while updating HR status." });
            }

            return Ok(new { success = true, message = "HR account status updated successfully!" });
        }
        #endregion

        #region EmployeeDirectory
        /// <summary>
        /// Registers a new regular Employee account and explicitly assigns Role_Id = Employee(3).
        /// </summary>
        /// <param name="model">The registration data payload for the new employee account.</param>
        /// <returns>A 200 OK status on success, or a 400 BadRequest if registration fails.</returns>
        [HttpPost]
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> RegisterEmployeeAccount([FromBody] UserRegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Data is null" });
            }

            model.Role_Id = (int)UserRole.Employee;

            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Invalid data format" });
            }

            var result = await _employeeService.RegisterNewUserAccountAsync(model);

            if (!result.Success)
            {
                return BadRequest(new { success = false, message = result.Message });
            }

            return Ok(new { success = true, message = result.Message });
        }

        /// <summary>
        /// Fetches existing data for a specific user to display inside the Edit Profile Popup modal.
        /// </summary>
        /// <param name="userId">The unique identifier of the target employee/HR.</param>
        /// <param name="isHRDirectory">Flag to track if the request comes from the HR Directory UI page.</param>
        /// <param name="isMyTeam">Flag to track if the request comes from the Team management UI page.</param>
        /// <returns>A partial view containing the profile update form fields pre-filled with data.</returns>
        [HttpGet]
        public async Task<IActionResult> GetEditProfilePopup(int userId, bool isHRDirectory = false, bool isMyTeam = false)
        {
            if (userId <= 0)
            {
                return BadRequest("Invalid User ID");
            }

            var employeeData = await _employeeService.GetHRForEditAsync(userId);
            if (employeeData == null)
            {
                return NotFound(new { message = "Employee not found." });
            }

            ViewData["IsHRDirectory"] = isHRDirectory;
            ViewData["IsMyTeam"] = isMyTeam;

            return PartialView("/Views/Employee/EditProfilePopup.cshtml", employeeData);
        }
        #endregion
    }
}