using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
namespace HRMS.Services
{
    /// <summary>
    /// Service for handling leave request business logic, including filtering, status updates, and reporting.
    /// </summary>
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;
        public LeaveService(AppDbContext context) => _context = context;

        /// <summary>
        /// Retrieves a filtered and paginated list of leave requests.
        /// </summary>
        /// <param name="mode">The filtering mode (e.g., "PendingRequests").</param>
        /// <param name="month">Optional month filter.</param>
        /// <param name="year">Optional year filter.</param>
        /// <param name="page">Current page number.</param>
        /// <param name="pageSize">Number of records per page.</param>
        /// <returns>A tuple containing the list of leave requests, total record count, and total page count.</returns>
        public async Task<(IEnumerable<LeaveRequestViewModel> Items, int TotalRecords, int TotalPages)> GetFilteredLeavesAsync(string mode, int? month, int? year, int page, int pageSize)
        {
            var query = _context.LeaveRequests.Include(l => l.User).Include(l => l.LeaveType).AsQueryable();

            if (mode == "PendingRequests")
            {
                query = query.Where(l => l.status == "Pending");
            }
            else
            {
                query = query.Where(l => l.status != "Pending");
                if (month.HasValue) query = query.Where(l => l.Start_Date.Month == month);
                if (year.HasValue) query = query.Where(l => l.Start_Date.Year == year);
            }
            int totalRecords = await query.CountAsync();
            var projectedQuery = query.Select(l => new LeaveRequestViewModel
            {
                LeaveRequestId = l.Leave_Request_Id,
                User_Id = l.User.User_Id,
                User_Name = l.User.User_Name,
                Department = l.User.Department.DepartmentName,
                LeaveType = l.LeaveType.Leave_Name,
                StartDate = l.Start_Date.ToString("MM/dd/yyyy"),
                EndDate = l.end_date.ToString("MM/dd/yyyy"),
                DateRange = $"{l.Start_Date:MMM dd} - {l.end_date:MMM dd, yyyy}",
                TotalDays = l.total_days,
                Status = l.status,
                Reason = l.reason,
                Attachment = l.attachment
            });
            var pagedItems = await projectedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            int totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
            return (pagedItems, totalRecords, totalPages);

        }
        // <summary>
        /// Rejects a pending leave request and restores the user's leave balance.
        /// </summary>
        /// <param name="leaveRequestId">The ID of the leave request to reject.</param>
        /// <param name="remark">The reason for rejection.</param>
        /// <returns>True if successful, otherwise false.</returns>Leave Decision a twat par 
         public async Task<bool> RejectLeaveRequestAsync(int leaveRequestId, string remark)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = await _context.LeaveRequests.FindAsync(leaveRequestId);
                    if (request == null || request.status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    var balance = await _context.LeaveBalances
                        .Where(b => b.User_Id == request.User_Id &&
                                    b.Leave_Type_Id == request.Leave_Type_Id)
                        .OrderByDescending(b => b.Year)
                        .FirstOrDefaultAsync();

                    if (balance != null)
                    {
                        balance.Used_Days -= request.total_days;
                        balance.Remaining_Days += request.total_days;
                        balance.updated_at = DateTime.UtcNow;
                    }

                    request.status = "Rejected";
                    request.remark = remark;
                    request.approved_at = DateTime.UtcNow;
                    request.updated_at = DateTime.UtcNow;

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }
        }

        public bool UpdateStatus(DecisionViewModel model)
        {
            var leave = _context.LeaveRequests.Find(model.LeaveRequestId);
            if (leave == null)
                return false;

            // Reject ကို ဒီ method က မလုပ်တော့ဘူး
            if (model.Status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
                return false;

            leave.status = model.Status;
            leave.remark = model.Remark;
            leave.approved_at = DateTime.UtcNow;

            _context.SaveChanges();
            return true;
        }


        /// <summary>
        /// Generates an Excel file stream from a list of leave requests.
        /// </summary>
        /// <param name="leaves">The list of leave requests to export.</param>
        /// <returns>A byte array representing the generated Excel file.</returns>
        public byte[] ExportLeaveHistoryToExcel(List<LeaveRequestViewModel> leaves)
        {
            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Leave History");

                string[] headers = { "Employee ID", "Employee Name", "Department", "Leave Type", "Date Range", "Total Days", "Leave Status" };

                for (int col = 0; col < headers.Length; col++)
                {
                    var cell = worksheet.Cells[1, col + 1];
                    cell.Value = headers[col];

                    cell.Style.Font.Bold = true;
                    cell.Style.Font.Size = 11;
                    cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));
                    cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int i = 0; i < leaves.Count; i++)
                {
                    var item = leaves[i];
                    int row = i + 2;

                    string formattedEmpId = $"EMP{item.User_Id.ToString().PadLeft(3, '0')}";

                    worksheet.Cells[row, 1].Value = formattedEmpId;
                    worksheet.Cells[row, 2].Value = item.User_Name;
                    worksheet.Cells[row, 3].Value = item.Department;
                    worksheet.Cells[row, 4].Value = item.LeaveType;
                    worksheet.Cells[row, 5].Value = item.DateRange;
                    worksheet.Cells[row, 6].Value = item.TotalDays;
                    worksheet.Cells[row, 7].Value = item.Status;

                    for (int col = 1; col <= headers.Length; col++)
                    {
                        var cell = worksheet.Cells[row, col];
                        cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        cell.Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        if (col == 1 || col == 6 || col == 7)
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        }
                        else
                        {
                            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                        }
                    }
                }

                worksheet.Row(1).Height = 25;
                if (worksheet.Dimension != null)
                {
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                }

                return package.GetAsByteArray();
            }
        }
    }
}

