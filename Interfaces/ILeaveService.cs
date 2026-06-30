using HRMS.Data.Entities;
using HRMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{

    public interface ILeaveService
    {
        IEnumerable<LeavePolicy> GetAllPolicies();
        IEnumerable<LeaveType> GetAllLeaveTypes();
        Task CreatePolicy(LeavePolicy model);
        Task<IEnumerable<LeaveType>> GetAllLeaveTypesAsync();
        Task CreateLeaveRequestAsync(LeaveRequest request);
        Task EnsureLeaveBalanceExistsAsync(int userId);
        Task ProcessAnniversaryCarryForward();
        Task<IEnumerable<LeaveDashboardViewModel>> GetDashboardBalancesAsync(int userId);
        Task<List<LeaveDashboardViewModel>> GetLeaveHistoryAsync(int userId);
        int GetCurrentMonth();
        int GetCurrentYear();
        Task<IEnumerable<LeaveDashboardViewModel>> GetLeaveHistoryFilteredAsync(int uId, int? month, int year);
        (IEnumerable<LeaveDashboardViewModel> List, int TotalCount) GetLeaveHistory(int userId, int m, int y, int page, int pageSize);
        Task<List<object>> GetLeaveRequestsWithDetails();
        Task<bool> DeleteLeaveRequestAsync(int leaveRequestId);
    }
}