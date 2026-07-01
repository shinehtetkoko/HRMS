using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HRMS.Controllers
{
    /// <summary>
    /// Manages organizational team structures, profile retrievals, and administrative employee data updates.
    /// </summary>
    public class TeamController : Controller
    {
        private readonly IMyTeamService _teamService;
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamController"/> class with tracking context dependencies.
        /// </summary>
        /// <param name="teamService">The service layer handle utilized for team structural operations.</param>
        /// <param name="context">The database context instance used for data query executions.</param>
        public TeamController(IMyTeamService teamService, AppDbContext context)
        {
            _teamService = teamService;
            _context = context;
        }

        /// <summary>
        /// Dynamically retrieves and renders the team tracking dashboard records based on specified status matrices.
        /// </summary>
        /// <param name="status">The structural employee lifecycle status criteria used for segmentation, defaulting to "All".</param>
        /// <param name="page">The explicit active page segment index configuration utilized for pagination, defaulting to 1.</param>
        /// <returns>A web view containing the team dashboard data collection constraints.</returns>
        [HttpGet]
        
        public async Task<IActionResult> MyTeam(string status = "All", int page = 1)
        {
            var model = await _teamService.GetMyTeamDashboardAsync(status, null, page);
            return View(model);
        }

        /// <summary>
        /// Fetches extensive database metrics for a single employee identity and maps fields into a modal partial view.
        /// </summary>
        /// <param name="id">The raw alphanumeric employee sequence string key expected for processing.</param>
        /// <returns>A partial html snippet representation view populated with structural profile details, or content alerts.</returns>
        [HttpGet]
        public async Task<IActionResult> GetEmployeeDetails(string id)
        {
            if (string.IsNullOrEmpty(id)) return Content("ID missing");
            string cleanId = id.Replace("EMP_", "").Trim();

            if (int.TryParse(cleanId, out int empId))
            {
                var emp = await _context.Users
                    .Include(u => u.Department)
                    .FirstOrDefaultAsync(u => u.User_Id == empId);

                if (emp != null)
                {
                    var account = await _context.UserAccounts.FirstOrDefaultAsync(a => a.User_Id == emp.User_Id);

                    var dto = new UserEditDto
                    {
                        User_Id = emp.User_Id,
                        User_Name = emp.User_Name,
                        Gender = emp.Gender,
                        Dob = emp.Dob,
                        Nrc = emp.Nrc,
                        Married_Status = emp.Married_Status,
                        Position = emp.Position,
                        DepartmentName = emp.Department != null ? emp.Department.DepartmentName : "No Department",
                        Hired_Date = emp.Hired_Date,
                        Qualification = emp.Qualification,
                        User_Ph_No = emp.User_Ph_No,
                        Email = account != null ? account.Email : "No Email",
                        Address = emp.Address
                    };

                    ViewData["IsMyTeam"] = true;
                    return PartialView("~/Views/Employee/EditProfilePopup.cshtml", dto);
                }
            }
            return Content("Employee not found");
        }

        /// <summary>
        /// Updates the persistence storage states for localized corporate profile parameters inside data tables.
        /// </summary>
        /// <param name="model">The data transfer object data bundle containing edited tracking fields.</param>
        /// <returns>A permanent dashboard view route redirection mapping sequence layout.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserEditDto model)
        {
            try
            {
                var emp = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == model.User_Id);
                if (emp != null)
                {
                    emp.User_Name = model.User_Name;
                    emp.User_Ph_No = model.User_Ph_No;
                    emp.Address = model.Address;
                    _context.Users.Update(emp);
                    await _context.SaveChangesAsync();
                }
                return RedirectToAction("MyTeam");
            }
            catch (Exception)
            {
                return RedirectToAction("MyTeam");
            }
        }
    }
}
