using Microsoft.EntityFrameworkCore;
using HRMS.Models;
using HRMS.Models.Admin;
using HRMS.Models.Employee;
using HRMS.Data;

namespace HRMS.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _context;

        public DashboardService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Display count of total departments, registered HRs, and active employees in the system.
        /// </summary>
        /// <returns>The view containing registered HR users.</returns>
        public async Task<AdminDashboardViewModel> GetAdminDashboardDataAsync()
        {
            var viewModel = new AdminDashboardViewModel();
            viewModel.TotalDepartments = await _context.Departments.CountAsync();
            viewModel.ActiveEmployees = await _context.Users.CountAsync(e => e.Is_Active);

            var hrUsers = await _context.UserAccounts
                .Include(u => u.User)
                .ThenInclude(u => u.Department)
                .Where(u => u.Role_Id == 2)
                .ToListAsync();

            viewModel.RegisteredHRsCount = hrUsers.Count;

            viewModel.RegisteredHRs = hrUsers.Select(h => new HRUserViewModel
            {
                HRId = $"HR{h.User_Id:D2}",
                HRName = h.User != null ? h.User.User_Name : "Unknown",
                DepartmentName = (h.User != null && h.User.Department != null) ? h.User.Department.DepartmentName : "N/A",
                Email = h.Email ?? "hr@hrsystem.com",
                Is_Active = h.User != null ? h.User.Is_Active : false
            }).ToList();

            return viewModel;
        }

        /// <summary>
        /// Display count of total employees, dpeartments, and daily leaves and pending actions.
        /// </summary>
        /// <returns>The view containing summary statistics.</returns>
        public async Task<HRDashboardViewModel> GetHRDashboardDataAsync()
        {
            var today = DateTime.UtcNow.Date;

            int totalEmployees = await _context.Users.CountAsync(u => u.Is_Active);
            int totalDepartments = await _context.Departments.CountAsync();

            int onLeaveToday = await _context.LeaveRequests
                .CountAsync(l => l.status == "Approved" && today >= l.Start_Date.Date && today <= l.end_date.Date);

            int pendingLeaves = await _context.LeaveRequests.CountAsync(l => l.status == "Pending");
            int pendingProfiles = await _context.ProfileUpdateRequests.CountAsync(p => p.Status == "Pending");

            return new HRDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                TotalDepartments = totalDepartments,
                TotalOnLeaveToday = onLeaveToday,
                PendingLeaveRequests = pendingLeaves,
                PendingProfileRequests = pendingProfiles,
                TotalPendingActions = pendingLeaves + pendingProfiles,
            };
        }

    }
}