using HRMS.Models.Admin;
using HRMS.Data.Entities;
using HRMS.Models.Holiday;
using System.Threading.Tasks;
using System.Collections.Generic;


namespace HRMS.Interfaces
{
    public interface ICompanyService
    {
        Task<CompanyProfileViewModel?> GetCompanyProfileAsync();

        Task<(bool Success, string Message)> UpdateCompanyProfileAsync(CompanyProfileViewModel model);

        Task<bool> ConfigurePublicHolidaysAsync(HolidayViewModel model, int userId);

        Task<List<PublicHoliday>> GetAllPublicHolidaysAsync();
    }
}