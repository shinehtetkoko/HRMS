using HRMS.Data;
using HRMS.Interfaces;
using HRMS.Data.Entities;
using HRMS.Models.Admin;
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
                Console.WriteLine($"Error in CompanyService: {ex.Message}");
                return (false, "Internal server error occurred while updating profile.");
            }
        }
    }
}