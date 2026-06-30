using HRMS.Interfaces;
using HRMS.Models;
using HRMS.Models.Employee;
using HRMS.Services;
using Microsoft.AspNetCore.Mvc;
namespace HRMS.Controllers
{
    public class LeaveController : Controller
    {
        private readonly ILeaveService _leaveService;
        public LeaveController(ILeaveService leaveService) => _leaveService = leaveService;

        public IActionResult LeaveRules()
        {
            return View();
        }
        public IActionResult LeaveHistory()
        {
            return View();
        }

        #region LeaveManagement

        /// <summary> Displays the main leave management dashboard.</summary>
        /// <returns>A view for leave management.</returns>
        public IActionResult LeaveManagement()
        {
            return View();
        }
        /// <summary>Renders the index view with filtered leave requests.</summary>
        /// <param name="mode">The current view mode.</param>
        /// <param name="month">month filter</param>
        /// <param name="year">year filter</param>
        /// <param name="page">The current page number.</param>
        /// <returns>A view containing the list of filtered leave requests.</returns>
        public async Task<IActionResult> Index(string mode = "PendingRequests", int? month = null, int? year = null, int page = 1)
        {
            int pageSize = 1;
            var (items, totalRecords, totalPages) = await _leaveService.GetFilteredLeavesAsync(mode, month, year, page, pageSize);
            ViewBag.SelectedMode = mode;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedYear = year;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalRecords = totalRecords;
            ViewBag.PageSize = pageSize;
            return View(items);
        }

        /// <summary>Fetches filtered leave data as JSON for dynamic table updates.</summary>
        /// <param name="mode">The current view mode.</param>
        /// <param name="month">month filter.</param>
        /// <param name="year">year filter.</param>
        /// <param name="page">The current page number.</param>
        /// <param name="pageSize">The number of records per page.</param>
        /// <returns>A JSON object containing the leave items and pagination metadata.</returns>
        [HttpGet]
        public async Task<IActionResult> GetLeaveRequests(string mode, int? month, int? year, int page = 1, int pageSize = 1)
        {
            var (items, totalRecords, totalPages) = await _leaveService.GetFilteredLeavesAsync(mode, month, year, page, pageSize);

            return Json(new
            {
                items = items,
                currentPage = page,
                totalRecords = totalRecords,
                totalPages = totalPages
            });
        }
        // <summary>
        /// Handles the rejection of a leave request by a manager.
        /// </summary>
        /// <param name="id">The unique identifier of the leave request.</param>
        /// <param name="remark">The reason or comment provided by the manager for the rejection.</param>
        /// <returns>
        /// A JSON object containing the status of the operation:
        /// <para>- <c>success</c>: Boolean indicating if the rejection was successful.</para>
        /// <para>- <c>message</c>: A string describing the result of the operation.</para>
        /// </returns> Leave Decision a twat par 
        [HttpPost]
        public async Task<IActionResult> UpdateLeaveStatus([FromBody] Models.DecisionViewModel model)
        {
            bool result;

            if (model.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                result = await _leaveService.RejectLeaveRequestAsync(model.LeaveRequestId, model.Remark);
            }
            else
            {
                result = _leaveService.UpdateStatus(model);
            }

            return Json(new { success = result });
        }

        /// <summary>Exports filtered leave history data to an Excel file.</summary>
        /// <param name="mode">The view mode to export.</param>
        /// <param name="month">month filter</param>
        /// <param name="year">year filter</param>
        /// <returns>A file result containing the Excel document.</returns>
        [HttpGet]
        public async Task<IActionResult> ExportLeaveToExcel(string mode, int? month, int? year)
        {
            var (items, _, _) = await _leaveService.GetFilteredLeavesAsync(mode, month, year, page: 1, pageSize: int.MaxValue);

            var data = items.ToList();

            byte[] fileContents = _leaveService.ExportLeaveHistoryToExcel(data);

            string fileName = $"Leave_History_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(
                fileContents,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        #endregion  LeaveManagement
    }
}