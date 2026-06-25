using HRMS.Models.Admin;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyProfileViewModel?> GetCompanyProfileAsync();
        Task<(bool Success, string Message)> UpdateCompanyProfileAsync(CompanyProfileViewModel model);
    }
}