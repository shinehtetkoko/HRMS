using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Services
{
    /// <summary>
    /// Provides data retrieval operations and analytical processing logic for administrative team management dashboards.
    /// </summary>
    public class MyTeamService : IMyTeamService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamService"/> class with context pipeline dependencies.
        /// </summary>
        /// <param name="context">The database context pipeline utilized for query execution targets.</param>
        public MyTeamService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Compiles total analytical counts and fetches a paginated subset of employee records based on active filters.
        /// </summary>
        /// <param name="status">The structural employee operational status criteria used for isolation routing.</param>
        /// <param name="selectedEmployeeId">The optional specific employee identification key used for target profile data hydration.</param>
        /// <param name="page">The current target page slice index configured for structural query skipping.</param>
        /// <returns>A team view model populated with operational summary statistics and paged employee arrays.</returns>
        public async Task<MyTeamViewModel> GetMyTeamDashboardAsync(string status, string? selectedEmployeeId, int page)
        {
            int pageSize = 10;

            var model = new MyTeamViewModel
            {
                SelectedStatus = string.IsNullOrEmpty(status) ? "All" : status,
                CurrentPage = page,
                PageSize = pageSize
            };

            model.PendingLeaveCount = await _context.LeaveRequests.CountAsync(l => l.status == "Pending");

            model.TeamMembersCount = await _context.Users.CountAsync(u => u.Is_Active);

            var todayUtc = DateTime.UtcNow.Date;
            model.OnLeaveTodayCount = await _context.LeaveRequests
                .CountAsync(l => l.status == "Approved" && todayUtc >= l.Start_Date && todayUtc <= l.end_date);

            var query = _context.Users.OrderBy(u => u.User_Id).AsQueryable();

            if (model.SelectedStatus == "Leave")
            {
                var leaveUserIds = await _context.LeaveRequests
                    .Where(l => l.status == "Approved" && todayUtc >= l.Start_Date && todayUtc <= l.end_date)
                    .Select(l => l.User_Id)
                    .ToListAsync();

                query = query.Where(u => leaveUserIds.Contains(u.User_Id));
            }

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            model.TotalRecords = totalRecords;
            model.TotalPages = totalPages;

            model.Employees = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new EmployeeDto
                {
                    EmployeeId = u.User_Id.ToString(),
                    EmployeeName = u.User_Name,
                    Department = u.Department != null ? u.Department.DepartmentName : "No Department"
                }).ToListAsync();

            if (!string.IsNullOrEmpty(selectedEmployeeId) && int.TryParse(selectedEmployeeId, out int empId))
            {
                var emp = await _context.Users
                    .FirstOrDefaultAsync(u => u.User_Id == empId);

                if (emp != null)
                {
                    model.SelectedEmployee = new UserEditDto
                    {
                        User_Id = emp.User_Id,
                        User_Name = emp.User_Name,
                        Position = emp.Position,
                        User_Ph_No = emp.User_Ph_No,
                        Address = emp.Address
                    };
                }
            }

            return model;
        }
    }
}
