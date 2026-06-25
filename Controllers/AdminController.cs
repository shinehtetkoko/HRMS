using Microsoft.AspNetCore.Mvc;
using HRMS.Interfaces;
using HRMS.Models.Employee;
using HRMS.Models.Admin;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Controllers
{
    public class AdminController : Controller
    {
        private readonly ICompanyService _companyService;
        private readonly IEmployeeService _employeeService;

        public AdminController(ICompanyService companyService, IEmployeeService employeeService)
        {
            _companyService = companyService;
            _employeeService = employeeService;
        }

        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> CompanyProfile()
        {
            var companyViewModel = await _companyService.GetCompanyProfileAsync();
            return View(companyViewModel ?? new CompanyProfileViewModel());
        }

        [HttpPost]
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

        public async Task<IActionResult> HRDirectory(string status = "Active")
        {
            var hrList = await _employeeService.GetHRDirectoryListAsync(status);

            ViewBag.CurrentStatus = status;

            return View(hrList);
        }

        // For Employee Directory
        public async Task<IActionResult> EmployeeDirectory(string status = "Active")
        {
            var employeeList = await _employeeService.GetEmployeeDirectoryListAsync(status);

            ViewBag.CurrentStatus = status;

            return View("~/Views/Employee/EmployeeDirectory.cshtml", employeeList);
        }

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

        // For Employee
        [HttpPost]
        public async Task<IActionResult> RegisterEmployeeAccount([FromBody] UserRegisterViewModel model)
        {
            if (model == null)
            {
                return BadRequest(new { success = false, message = "Data is null" });
            }

            model.Role_Id = 3;

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

    }
}