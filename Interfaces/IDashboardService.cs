using HRMS.Models.Admin;
using HRMS.Models.Employee;
using HRMS.Data.Entities;

namespace HRMS.Services
{
    public interface IDashboardService
    {
        Task<AdminDashboardViewModel> GetAdminDashboardDataAsync();

        Task<HRDashboardViewModel> GetHRDashboardDataAsync();
    }
}
