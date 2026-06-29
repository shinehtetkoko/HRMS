using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Data.Entities;
using HRMS.Models.Admin;
using HRMS.Models.Holiday;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace HRMS.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly AppDbContext _context;

        public CompanyService(AppDbContext context)
        {
            _context = context;
        }

        #region Company Profile Operations
        /// <summary>
        /// Fetches the company profile record from the database and maps it to a ViewModel.
        /// </summary>
        /// <returns>The CompanyProfileViewModel containing company details, or null if no record exists.</returns>
        public async Task<CompanyProfileViewModel?> GetCompanyProfileAsync()
        {
            var company = await _context.Companies.FirstOrDefaultAsync();
            if (company == null) return null;

            return new CompanyProfileViewModel
            {
                Comp_Id = company.Comp_Id,
                Comp_Name = company.Comp_Name,
                Comp_Ph_No = company.Comp_Ph_No,
                Comp_Email = company.Comp_Email,
                Comp_Location = company.Comp_Location,
                Description = company.Description,
                Office_Start_Time = company.Office_Start_Time,
                Office_End_Time = company.Office_End_Time
            };
        }

        /// <summary>
        /// Updates the existing company profile details with the new data submitted by the admin.
        /// </summary>
        /// <param name="model">The data payload containing the modified company profile details.</param>
        /// <returns>A tuple indicating success status and a feedback message.</returns>
        public async Task<(bool Success, string Message)> UpdateCompanyProfileAsync(CompanyProfileViewModel model)
        {
            try
            {
                var existingCompany = await _context.Companies.FirstOrDefaultAsync(c => c.Comp_Id == model.Comp_Id);

                if (existingCompany == null)
                {
                    return (false, "Company profile not found");
                }

                existingCompany.Comp_Ph_No = model.Comp_Ph_No;
                existingCompany.Comp_Email = model.Comp_Email;
                existingCompany.Comp_Location = model.Comp_Location;
                existingCompany.Description = model.Description;

                existingCompany.updated_at = DateTime.UtcNow;

                _context.Companies.Update(existingCompany);
                await _context.SaveChangesAsync();

                return (true, "Company profile updated successfully!");
            }
            catch (Exception ex)
            {      
                return (false, "Internal server error occurred while updating profile.");
            }
        }
        #endregion

        #region HolidaySetup
        /// <summary>
        /// Save new holiday, check for duplicates, and handle recurring holidays.
        /// </summary>
        /// <param name="model">The view model containing holiday configuration details.</param>
        /// <param name="userId">The user ID who create the holiday record.</param>
        /// <returns>True if the holiday was saved successfully.</returns>
        public async Task<bool> ConfigurePublicHolidaysAsync(HolidayViewModel model, int userId)
        {
            // mapping with entity
            var holiday = new PublicHoliday
            {
                Holiday_Name = model.Holiday_Name,
                Holiday_Type = model.Holiday_Type,
                Start_Date = model.Start_Date,
                End_Date = model.End_Date,
                Is_Recurring = model.Is_Recurring,
                Created_At = DateTime.UtcNow,
                Created_By_User_Id = userId
            };

            // check duplicate
            var isDuplicate = await _context.PublicHolidays.AnyAsync(h => h.Start_Date == holiday.Start_Date);
            if (isDuplicate) return false;

            _context.PublicHolidays.Add(holiday);

            //recurring logic
            if (holiday.Is_Recurring == true)
            {
                var nextYearHoliday = new PublicHoliday
                {
                    Holiday_Name = holiday.Holiday_Name + " (Recurring)",
                    Holiday_Type = holiday.Holiday_Type,
                    Start_Date = holiday.Start_Date.AddYears(1), // for next year
                    End_Date = holiday.End_Date.AddYears(1),
                    Is_Recurring = true,
                    Created_At = DateTime.UtcNow,
                    Created_By_User_Id = holiday.Created_By_User_Id
                };
                _context.PublicHolidays.Add(nextYearHoliday);
            }
            return await _context.SaveChangesAsync() > 0;
        }

        /// <summary>
        /// Display lists of holidays.
        /// </summary>
        /// <returns>A list of holiay entities.</returns>
        public async Task<List<PublicHoliday>> GetAllPublicHolidaysAsync()
        {
            return await _context.PublicHolidays
            .Include(t => t.Creator)
            .OrderBy(h => h.Start_Date)
            .ToListAsync();
        }
        #endregion
    }
}