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

        Task<ProfileViewModel> GetProfileDataAsync(int userId);

        Task<bool> SubmitProfileUpdateRequestAsync(UpdateProfileRequestViewModel request);

        Task<bool> ApproveProfileUpdateAsync(int userId, int reviewedByUserId);

        Task<IEnumerable<string>> GetDepartmentNamesAsync();

        Task<PagedResultViewModel<EmployeeDirectoryViewModel>> GetFilteredEmployeesAsync(string status, string department, int page, int pageSize);

        Task<byte[]> ExportEmployeesToExcelAsync(string status, string department);

        Task<(List<EmployeeImportDtoViewModel> ValidData, List<EmployeeImportDtoViewModel> Errors)> ReadExcelAsync(IFormFile file);

        Task<(bool Success, List<EmployeeImportDtoViewModel> Errors)> ImportEmployeesFromExcel(List<EmployeeImportDtoViewModel> dtos);

        MemoryStream GenerateErrorExcel(List<EmployeeImportDtoViewModel> errors);
    }
}