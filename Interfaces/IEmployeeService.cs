using HRMS.Models.Employee;
using HRMS.Models.Admin;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IEmployeeService
    {
        Task<(bool Success, string Message)> RegisterNewUserAccountAsync(UserRegisterViewModel model);

        Task<IEnumerable<HRDirectoryViewModel>> GetHRDirectoryListAsync(string status);

        Task<HRUpdateViewModel> GetHRForEditAsync(int userId);

        Task<bool> UpdateHRStatusAsync(HRUpdateViewModel model, int currentAdminId);

        Task<IEnumerable<HRDirectoryViewModel>> GetEmployeeDirectoryListAsync(string status);
    }
}