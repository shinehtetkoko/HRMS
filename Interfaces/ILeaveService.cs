using HRMS.Models.Employee;
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
    }
}
