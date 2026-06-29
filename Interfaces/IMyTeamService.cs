using HRMS.Models;

namespace HRMS.Interfaces
{
    public interface IMyTeamService
    {
        Task<MyTeamViewModel> GetMyTeamDashboardAsync(string status, string? selectedEmployeeId, int page);
    }
}
