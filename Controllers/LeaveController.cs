using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HRMS.Controllers
{
    /// <summary>
    /// Manages leave-related operations including leave applications,
    /// leave history, leave balance management, policy configuration,
    /// file attachments, and anniversary carry-forward processing.
    /// </summary>
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        private readonly AppDbContext _context;
        private readonly IAuditLogService _auditLogService;

        /// <summary>
        /// Initializes a new instance of the LeaveController.
        /// </summary>
        /// <param name="leaveService">Provides leave management business logic.</param>
        /// <param name="context">Database context for data access.</param>
        public LeaveController(ILeaveService leaveService, AppDbContext context, IAuditLogService auditLogService)
        {
            _leaveService = leaveService;
            _context = context;
            _auditLogService = auditLogService;
        }

        #region Leave Policy
        /// <summary>
        /// Displays the leave policy setup page.
        /// </summary>
        /// <returns>Leave setup view with leave types and policies.</returns>
        public IActionResult LeaveSetup()
        {
            var viewModel = new LeaveSetupViewModel { LeavePolicies = _leaveService.GetAllPolicies(), LeaveTypes = _leaveService.GetAllLeaveTypes() };
            return View(viewModel);
        }

        /// <summary>
        /// Creates a new leave policy.
        /// </summary>
        /// <param name="model">Leave policy information.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> CreateLeavePolicy(LeavePolicy model)
        {
            try
            {
                await _leaveService.CreatePolicy(model);
                int performedAccountId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value?? User.FindFirst("UserId")?.Value;
                if (userIdClaim != null)
                {
                    performedAccountId = int.Parse(userIdClaim);
                    await _auditLogService.AddLogAsync(performedAccountId, model, "LeaveSetup", "Created");
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion

        public IActionResult HolidaySetup()
        {
            return View();
        }

        #region Leave Request
        /// <summary>
        /// Submits a new leave request.
        /// Performs balance validation, policy validation,
        /// attachment validation, and updates leave balances.
        /// </summary>
        /// <param name="model">Leave request information.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Apply([FromForm] LeaveRequestViewModel model)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { success = false, message = "User not found." });
            }
            int uId = int.Parse(userIdClaim);
            await _leaveService.EnsureLeaveBalanceExistsAsync(uId);
            var balance = await _context.LeaveBalances.Include(b => b.LeaveType).Where(b => b.User_Id == uId && b.Leave_Type_Id == model.LeaveTypeId).OrderByDescending(b => b.Year).FirstOrDefaultAsync();
            if (balance == null)
            {
                return Json(new { success = false, message = "Leave balance not found." });
            }
            if (balance.Remaining_Days <= 0)
            {
                return Json(new { success = false, message = "Insufficient leave balance. You have 0 days remaining." });
            }
            if (model.TotalDays > balance.Remaining_Days)
            {
                return Json(new { success = false, message = $"Insufficient leave balance. You have only {balance.Remaining_Days} days." });
            }
            DateTime today = DateTime.UtcNow.Date;
            if (model.StartDate.Date < today)
            {
                return Json(new { success = false, message = "Invalid Start Date: You cannot select a date in the past." });
            }
            var leaveType = await _context.LeaveTypes.FindAsync(model.LeaveTypeId);
            if (leaveType == null)
            {
                return Json(new { success = false, message = "Invalid leave type." });
            }
            var policy = await _context.LeavePolicies.FirstOrDefaultAsync(p => p.Leave_Type_Id == model.LeaveTypeId);
            if (policy != null && model.TotalDays > policy.Total_Days)
            {
                return Json(new { success = false, message = $"The requested duration exceeds the limit of {policy.Total_Days} days." });
            }
            bool isMedical = leaveType.Leave_Name.Trim().Equals("Medical", StringComparison.OrdinalIgnoreCase);
            if (isMedical && (model.Attachment == null || model.Attachment.Length == 0))
            {
                return Json(new { success = false, message = "A medical certificate is required for medical leave." });
            }
            string? uniqueFileName = null;
            if (model.Attachment != null && model.Attachment.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }
                uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Attachment.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Attachment.CopyToAsync(fileStream);
                }
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = new LeaveRequest { User_Id = uId, Leave_Type_Id = model.LeaveTypeId, Start_Date = model.StartDate.Date, end_date = model.EndDate.Date, total_days = model.TotalDays, reason = model.Reason, status = "Pending", created_at = DateTime.UtcNow, attachment = uniqueFileName };
                    _context.LeaveRequests.Add(request);
                    balance.Used_Days += model.TotalDays;
                    balance.Remaining_Days -= model.TotalDays;
                    balance.updated_at = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    var updatedBalance = await _context.LeaveBalances.FirstOrDefaultAsync(b => b.User_Id == uId && b.Leave_Type_Id == model.LeaveTypeId);
                    return Json(new { success = true, message = "Leave request submitted successfully.", newBalance = updatedBalance?.Remaining_Days });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "An error occurred: " + ex.Message });
                }
            }
        }

        /// <summary>
        /// Updates an existing leave request and recalculates leave balances.
        /// </summary>
        /// <param name="model">Updated leave request information.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> Update([FromForm] LeaveRequestViewModel model)
        {
            DateTime today = DateTime.UtcNow.Date;
            if (model.StartDate.Date < today)
            {
                return Json(new { success = false, message = "You cannot select a date in the past." });
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = await _context.LeaveRequests.FindAsync(model.LeaveRequestId);
                    if (request == null)
                    {
                        return Json(new { success = false, message = "Request not found" });
                    }
                    if (request.status.Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        return Json(new { success = false, message = "Approved leave cannot be modified." });
                    }
                    var oldBalance = await _context.LeaveBalances.Where(b => b.User_Id == request.User_Id && b.Leave_Type_Id == request.Leave_Type_Id).OrderByDescending(b => b.Year).FirstOrDefaultAsync();
                    if (oldBalance != null)
                    {
                        oldBalance.Used_Days -= request.total_days;
                        oldBalance.Remaining_Days += request.total_days;
                        oldBalance.updated_at = DateTime.UtcNow;
                    }
                    var newBalance = await _context.LeaveBalances.Where(b => b.User_Id == request.User_Id && b.Leave_Type_Id == model.LeaveTypeId).OrderByDescending(b => b.Year).FirstOrDefaultAsync();
                    if (newBalance == null)
                    {
                        return Json(new { success = false, message = "Leave balance not found." });
                    }
                    if (model.TotalDays > newBalance.Remaining_Days)
                    {
                        return Json(new { success = false, message = $"Insufficient balance. Remaining days = {newBalance.Remaining_Days}" });
                    }
                    newBalance.Used_Days += model.TotalDays;
                    newBalance.Remaining_Days -= model.TotalDays;
                    newBalance.updated_at = DateTime.UtcNow;
                    request.Leave_Type_Id = model.LeaveTypeId;
                    request.Start_Date = model.StartDate.Date;
                    request.end_date = model.EndDate.Date;
                    request.total_days = model.TotalDays;
                    request.reason = model.Reason;
                    request.updated_at = DateTime.UtcNow;
                    string? uniqueFileName = null;
                    var leaveType = await _context.LeaveTypes.FindAsync(model.LeaveTypeId);
                    bool isMedical = leaveType != null && leaveType.Leave_Name.Trim().Equals("Medical", StringComparison.OrdinalIgnoreCase);
                    if (isMedical && model.Attachment != null)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                        uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(model.Attachment.FileName);
                        using (var fileStream = new FileStream(Path.Combine(uploadsFolder, uniqueFileName), FileMode.Create)) await model.Attachment.CopyToAsync(fileStream);
                        request.attachment = uniqueFileName;
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return Json(new { success = true, message = "Leave updated successfully." });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = "Error: " + ex.Message });
                }
            }
        }

        /// <summary>
        /// Deletes a pending leave request and restores leave balance.
        /// </summary>
        /// <param name="id">Leave request identifier.</param>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            bool isDeleted = await _leaveService.DeleteLeaveRequestAsync(id);
            if (isDeleted)
            {
                return Json(new { success = true, message = "Deletion completed successfully." });
            }
            else
            {
                return Json(new { success = false, message = "\"Unable to delete. The leave request may have already been approved." });
            }
        }
        #endregion

        #region Leave History
        /// <summary>
        /// Displays leave history and leave balance summary for the current user.
        /// </summary>
        /// <returns>Leave history view.</returns>
        public async Task<IActionResult> LeaveHistory()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            int uId = int.Parse(userIdClaim);
            var model = await _leaveService.GetDashboardBalancesAsync(uId);
            ViewBag.SelectedMonth = DateTime.Now.Month;
            ViewBag.SelectedYear = DateTime.Now.Year;
            ViewBag.LeaveTypes = await _leaveService.GetAllLeaveTypesAsync();
            ViewBag.LeavePolicies = await _context.LeavePolicies.ToListAsync();
            var list = await _leaveService.GetLeaveHistoryAsync(uId);
            foreach (var item in list)
            {
                Console.WriteLine($"ID={item.LeaveRequestId}, Attachment={item.Attachment}");
            }
            ViewBag.HistoryList = list ?? new List<HRMS.Models.LeaveDashboardViewModel>();
            return View(model);
        }

        /// <summary>
        /// Retrieves paginated leave history based on selected month and year.
        /// </summary>
        /// <param name="month">Selected month.</param>
        /// <param name="year">Selected year.</param>
        /// <param name="page">Page number.</param>
        /// <returns>JSON data for leave history pagination.</returns>
        [HttpGet]
        public async Task<IActionResult> GetFilteredHistory(int month, int year, int page = 1)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                int uId = int.Parse(userIdClaim);
                int pageSize = 10;
                var (list, totalCount) = _leaveService.GetLeaveHistory(uId, month, year, page, pageSize);
                var resultList = list ?? Enumerable.Empty<LeaveDashboardViewModel>();

                return Json(new { items = resultList, totalCount = totalCount, totalPages = (int)Math.Ceiling(totalCount / (double)pageSize), currentPage = page });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves all leave policies.
        /// </summary>
        /// <returns>JSON collection of leave policies.</returns>
        [HttpGet]
        public JsonResult GetLeavePolicies()
        {
            var policies = _leaveService.GetAllPolicies();
            return Json(policies);
        }
        #endregion

        #region Dashboard
        /// <summary>
        /// Displays dashboard information including leave balances.
        /// </summary>
        /// <returns>Dashboard view.</returns>
        public async Task<IActionResult> Dashboard()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return RedirectToAction("Login", "Account");
            }
            int uId = int.Parse(userIdClaim);
            await _leaveService.EnsureLeaveBalanceExistsAsync(uId);
            var model = await _leaveService.GetDashboardBalancesAsync(uId);
            return View(model);
        }

        /// <summary>
        /// Displays the default leave page.
        /// </summary>
        /// <returns>Index view.</returns>
        public IActionResult Index()
        {
            ViewBag.SelectedMonth = DateTime.Now.Month.ToString();
            ViewBag.SelectedYear = DateTime.Now.Year.ToString();
            return View();
        }
        #endregion

        #region Anniversary Process
        /// <summary>
        /// Executes anniversary carry-forward processing for all eligible employees.
        /// </summary>
        /// <returns>JSON result indicating success or failure.</returns>
        [HttpPost]
        public async Task<IActionResult> RunAnniversaryProcess()
        {
            try
            {
                await _leaveService.ProcessAnniversaryCarryForward();
                return Json(new { success = true, message = "Anniversary process completed." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Updates leave balance by applying carried-forward leave days.
        /// </summary>
        /// <param name="userId">User identifier.</param>
        /// <param name="leaveTypeId">Leave type identifier.</param>
        public async Task UpdateBalanceWithCarryForward(int userId, int leaveTypeId)
        {
            var balance = await _context.LeaveBalances.FirstOrDefaultAsync(b => b.User_Id == userId && b.Leave_Type_Id == leaveTypeId);
            var policy = await _context.LeavePolicies.FirstOrDefaultAsync(p => p.Leave_Type_Id == leaveTypeId);
            if (balance != null && policy != null)
            {
                int carryForwardDays = balance.Carried_Forward_Days ?? 0;
                balance.Remaining_Days = policy.Total_Days + carryForwardDays;
                await _context.SaveChangesAsync();
            }
        }
        #endregion

        #region File Attachment
        /// <summary>
        /// Downloads a leave attachment file.
        /// </summary>
        /// <param name="fileName">Attachment file name.</param>
        /// <returns>Physical file download response.</returns>
        public IActionResult DownloadFile(string fileName)
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", fileName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }
            return PhysicalFile(path, "application/octet-stream", fileName);
        }

        /// <summary>
        /// Downloads an attachment associated with a leave request.
        /// </summary>
        /// <param name="fileName">Attachment file name.</param>
        /// <returns>Physical file download response.</returns>
        [HttpGet]
        public IActionResult DownloadAttachment(string fileName)
        {
            string cleanFileName = fileName.Trim();
            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            string path = Path.Combine(uploadsFolder, cleanFileName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound("The file was not found on the server. The filename stored in the database might be incorrect.");
            }
            return PhysicalFile(path, "application/octet-stream", cleanFileName);
        }
        #endregion
    }
}