using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
namespace HRMS.Services
{
    /// <summary>
    /// Service class for handling all leave-related business logic, including policies, balance tracking, and request management.
    /// </summary>
    public class LeaveService : ILeaveService
    {
        private readonly AppDbContext _context;

        /// <summary> Initializes a new instance of <see cref="LeaveService"/>. </summary>
        public LeaveService(AppDbContext context) => _context = context;

        /// <summary> Retrieves all leave policies including their associated leave types. </summary>
        public IEnumerable<LeavePolicy> GetAllPolicies() => _context.LeavePolicies.Include(l => l.LeaveType).ToList();

        /// <summary> Retrieves all defined leave types. </summary>
        public IEnumerable<LeaveType> GetAllLeaveTypes() => _context.LeaveTypes.ToList();

        /// <summary> Creates a new leave policy with validation logic for carry-forward days. </summary>
        /// <param name="model">The leave policy model to create.</param>
        public void CreatePolicy(LeavePolicy model)
        {
            // Business logic for annual leave carry-forward settings
            if (model.Leave_Type_Id == 1)
            {
                model.Carry_Duration = 1;
                model.Carry_Forward = true;
            }
            else
            {
                model.Carry_Forward = false;
                model.Max_Carry_Day = 0;
                model.Carry_Duration = 0;
            }
            if (model.Leave_Type_Id != 1)
            {
                model.Carry_Forward = false;
                model.Max_Carry_Day = 0;
            }
            if (model.Total_Days < 1)
            {
                throw new Exception("Total Available Days must be 1 or greater.");
            }
            if (model.Leave_Type_Id == 1)
            {
                model.Carry_Duration = 1;
            }
            model.Created_At = DateTime.UtcNow;
            _context.LeavePolicies.Add(model);
            _context.SaveChangesAsync().Wait();
        }

        /// <summary> Asynchronously retrieves all available leave types. </summary>
        public async Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync() => await _context.LeaveTypes.ToListAsync();

        /// <summary> Asynchronously creates a new leave request record. </summary>
        /// <param name="request">The leave request model.</param>
        public async Task CreateLeaveRequestAsync(LeaveRequest request)
        {
            _context.LeaveRequests.Add(request);
            await _context.SaveChangesAsync();
        }

        /// <summary> Ensures that a leave balance record exists for the user for the current year. </summary>
        /// <param name="userId">The ID of the user.</param>
        public async Task EnsureLeaveBalanceExistsAsync(int userId)
        {
            var leaveTypes = await _context.LeaveTypes.ToListAsync();
            int currentYear = DateTime.UtcNow.Year;
            DateTime now = DateTime.UtcNow;
            foreach (var type in leaveTypes)
            {
                var exists = await _context.LeaveBalances.AnyAsync(b => b.User_Id == userId && b.Leave_Type_Id == type.Leave_Type_Id && b.Year == currentYear);
                if (!exists)
                {
                    var policy = await _context.LeavePolicies.FirstOrDefaultAsync(p => p.Leave_Type_Id == type.Leave_Type_Id);
                    int totalDays = policy != null ? policy.Total_Days : 0;
                    var newBalance = new LeaveBalance { User_Id = userId, Leave_Type_Id = type.Leave_Type_Id, Year = currentYear, Allocated_Days = totalDays, Used_Days = 0, Remaining_Days = totalDays, Created_At = now, };
                    _context.LeaveBalances.Add(newBalance);
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary> Processes anniversary-based leave carry-forward for employees and resets non-carry-forward leave balances. </summary>
        public async Task ProcessAnniversaryCarryForward()
        {
            var today = DateTime.UtcNow.Date;
            var usersToProcess = await _context.Users.Where(u => u.Hired_Date.Year < today.Year && u.Hired_Date.Month == today.Month && u.Hired_Date.Day == today.Day).ToListAsync();
            foreach (var user in usersToProcess)
            {
                var allBalances = await _context.LeaveBalances.Include(b => b.LeaveType).Where(b => b.User_Id == user.User_Id && b.Year == today.Year).ToListAsync();
                foreach (var balance in allBalances)
                {
                    var leaveName = balance.LeaveType.Leave_Name.ToUpper();
                    if (leaveName == "ANNUAL")
                    {
                        var policy = await _context.LeavePolicies.Include(p => p.LeaveType).FirstOrDefaultAsync(p => p.LeaveType.Leave_Name.ToUpper() == "ANNUAL");
                        var existingNewBalance = await _context.LeaveBalances.FirstOrDefaultAsync(b => b.User_Id == user.User_Id && b.Year == today.Year + 1 && b.Leave_Type_Id == balance.Leave_Type_Id);
                        if (policy != null && existingNewBalance == null)
                        {
                            int carryForwardDays = Math.Min(balance.Remaining_Days, policy.Max_Carry_Day ?? 0);
                            var newBalance = new LeaveBalance { User_Id = user.User_Id, Leave_Type_Id = balance.Leave_Type_Id, Year = today.Year + 1, Allocated_Days = policy.Total_Days, Used_Days = 0, Remaining_Days = policy.Total_Days + carryForwardDays, Carried_Forward_Days = carryForwardDays, Created_At = DateTime.UtcNow };
                            _context.LeaveBalances.Add(newBalance);
                        }
                    }
                    else
                    {
                        var nextYearBalance = await _context.LeaveBalances.FirstOrDefaultAsync(b => b.User_Id == user.User_Id && b.Year == today.Year + 1 && b.Leave_Type_Id == balance.Leave_Type_Id);
                        if (nextYearBalance == null)
                        {
                            var policy = await _context.LeavePolicies.FirstOrDefaultAsync(p => p.Leave_Type_Id == balance.Leave_Type_Id);
                            var newBalance = new LeaveBalance { User_Id = user.User_Id, Leave_Type_Id = balance.Leave_Type_Id, Year = today.Year + 1, Allocated_Days = policy?.Total_Days ?? 0, Used_Days = 0, Remaining_Days = policy?.Total_Days ?? 0, Created_At = DateTime.UtcNow };
                            _context.LeaveBalances.Add(newBalance);
                        }
                    }
                    balance.Remaining_Days = 0;
                    _context.LeaveBalances.Update(balance);
                }
            }
            await _context.SaveChangesAsync();
        }

        /// <summary> Gets the leave balance dashboard for a specific user based on the latest year available. </summary>
        /// <param name="userId">The ID of the user.</param>
        public async Task<IEnumerable<LeaveDashboardViewModel>> GetDashboardBalancesAsync(int userId)
        {
            var latestYear = await _context.LeaveBalances.Where(b => b.User_Id == userId).OrderByDescending(b => b.Year).Select(b => b.Year).FirstOrDefaultAsync();
            int targetYear = latestYear > 0 ? latestYear : DateTime.UtcNow.Year;
            return await _context.LeaveBalances.Include(b => b.LeaveType).Where(b => b.User_Id == userId && b.Year == targetYear).Select(b => new LeaveDashboardViewModel { LeaveName = b.LeaveType.Leave_Name, RemainingDays = b.Remaining_Days, TotalDays = b.Allocated_Days + (b.Carried_Forward_Days ?? 0) }).ToListAsync();
        }

        /// <summary> Retrieves the complete leave history for a specific user. </summary>
        /// <param name="userId">The user ID.</param>
        public async Task<List<LeaveDashboardViewModel>> GetLeaveHistoryAsync(int userId)
        {
            return await _context.LeaveRequests.Include(lr => lr.LeaveType).Include(lr => lr.User).ThenInclude(u => u.Department).Where(lr => lr.User_Id == userId).OrderByDescending(lr => lr.created_at).Select(lr => new LeaveDashboardViewModel { LeaveRequestId = lr.Leave_Request_Id, LeaveType = lr.LeaveType.Leave_Name, StartDate = lr.Start_Date, EndDate = lr.end_date, TotalDays = lr.total_days, Status = lr.status, Reason = lr.reason, EmployeeId = lr.User.User_Id, EmployeeName = lr.User.User_Name, DepartmentName = lr.User.Department.DepartmentName }).ToListAsync();
        }

        /// <summary> Gets the current month as an integer. </summary>
        public int GetCurrentMonth() => DateTime.Now.Month;

        /// <summary> Gets the current year as an integer. </summary>
        public int GetCurrentYear() => DateTime.Now.Year;

        /// <summary> Retrieves paginated leave history for a specific user and timeframe. </summary>
        public async Task<IEnumerable<LeaveDashboardViewModel>> GetLeaveHistoryFilteredAsync(int uId, int? month, int year)
        {
            var query = _context.LeaveRequests.Include(x => x.User).ThenInclude(u => u.Department).Include(x => x.LeaveType).Where(x => x.User_Id == uId && x.Start_Date.Year == year);
            if (month.HasValue)
            {
                query = query.Where(x => x.Start_Date.Month == month.Value);
            }
            return await query.Select(x => new LeaveDashboardViewModel { EmployeeId = x.User.User_Id, EmployeeName = x.User.User_Name, DepartmentName = (x.User != null && x.User.Department != null) ? x.User.Department.DepartmentName : "No Dept", LeaveType = x.LeaveType != null ? x.LeaveType.Leave_Name : "N/A", StartDate = x.Start_Date, EndDate = x.end_date, TotalDays = x.total_days, Status = x.status, Reason = x.reason }).ToListAsync();
        }

        /// <summary>
        /// Retrieves a paginated list of leave history for a specific user, filtered by month and year.
        /// </summary>
        /// <param name="userId">The ID of the user whose leave history is being retrieved.</param>
        /// <param name="m">The month (1-12) to filter the leave requests.</param>
        /// <param name="y">The year to filter the leave requests.</param>
        /// <param name="page">The current page number for pagination.</param>
        /// <param name="pageSize">The number of records to display per page.</param>
        /// <returns>
        /// A tuple containing:
        /// <para>- <c>List</c>: An enumerable collection of <see cref="LeaveDashboardViewModel"/> for the current page.</para>
        /// <para>- <c>TotalCount</c>: The total number of leave records matching the filter criteria.</para>
        /// </returns>
        public (IEnumerable<LeaveDashboardViewModel> List, int TotalCount) GetLeaveHistory(int userId, int m, int y, int page, int pageSize)
        {
            var query = _context.LeaveRequests.Where(x => x.User_Id == userId && x.Start_Date.Month == m && x.Start_Date.Year == y);
            int totalCount = query.Count();
            var list = query.OrderByDescending(x => x.Leave_Request_Id).Skip((page - 1) * pageSize).Take(pageSize).Select(x => new LeaveDashboardViewModel { LeaveRequestId = x.Leave_Request_Id, LeaveType = x.LeaveType.Leave_Name, StartDate = x.Start_Date, EndDate = x.end_date, TotalDays = x.total_days, Status = x.status, Reason = x.reason, EmployeeId = x.User.User_Id, EmployeeName = x.User.User_Name, DepartmentName = x.User.Department.DepartmentName, Attachment = x.attachment }).ToList();
            return (list, totalCount);
        }

        public async Task<List<object>> GetLeaveRequestsWithDetails()
        {
            var query = await _context.LeaveRequests.Include(l => l.User).ThenInclude(u => u.Department).OrderByDescending(l => l.created_at).Select(l => new { leaveRequestId = l.Leave_Request_Id, employeeId = l.User_Id, employeeName = l.User.User_Name, leaveType = l.LeaveType.Leave_Name, startDate = l.Start_Date, endDate = l.end_date, totalDays = l.total_days, reason = l.reason, status = l.status, attachment = l.attachment }).ToListAsync(); return query.Cast<object>().ToList();
        }

        /// <summary>
        /// Deletes an existing leave request and restores the corresponding leave balance.
        /// </summary>
        /// <remarks>
        /// This method uses a database transaction to ensure data integrity. 
        /// 1. Checks if the request exists and is not already 'Approved'.
        /// 2. Reverts the deduction from the user's leave balance (increases remaining days).
        /// 3. Removes the leave request record from the database.
        /// If any step fails, the transaction is rolled back.
        /// </remarks>
        /// <param name="leaveRequestId">The unique ID of the leave request to be deleted.</param>
        /// <returns>
        /// A task representing the asynchronous operation. Returns <c>true</c> if the request was successfully deleted; 
        /// otherwise, <c>false</c> (e.g., if the request is 'Approved' or does not exist).
        /// </returns>
        public async Task<bool> DeleteLeaveRequestAsync(int leaveRequestId)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var request = await _context.LeaveRequests.FindAsync(leaveRequestId);
                    if (request == null)
                    {
                        return false;
                    }

                    if (request.status.Trim().Equals("Approved", StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    var balance = await _context.LeaveBalances.Where(b => b.User_Id == request.User_Id && b.Leave_Type_Id == request.Leave_Type_Id).OrderByDescending(b => b.Year).FirstOrDefaultAsync();

                    if (balance != null)
                    {
                        Console.WriteLine($"Before Delete => Remaining={balance.Remaining_Days}");
                        balance.Used_Days -= request.total_days;
                        balance.Remaining_Days += request.total_days;
                        Console.WriteLine($"After Delete => Remaining={balance.Remaining_Days}");
                        balance.updated_at = DateTime.UtcNow;
                    }
                    _context.LeaveRequests.Remove(request);
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
    }
}