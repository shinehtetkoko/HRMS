using HRMS.Data.Entities;
using HRMS.Models;
using DecisionViewModel = HRMS.Models.DecisionViewModel;
namespace HRMS.Interfaces
{
    public interface ILeaveService
    {
        Task<(IEnumerable<LeaveRequestViewModel> Items, int TotalRecords, int TotalPages)> GetFilteredLeavesAsync(string mode, int? month, int? year, int page, int pageSize);
        bool UpdateStatus(DecisionViewModel model);
        byte[] ExportLeaveHistoryToExcel(List<LeaveRequestViewModel> leaves);
        Task<bool> RejectLeaveRequestAsync(int leaveRequestId, string remark);

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
