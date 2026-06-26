using BCrypt.Net;
using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Models.Employee;
using HRMS.Models.Admin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRMS.Enums;

namespace HRMS.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly IEmailService _emailService;

        public EmployeeService(AppDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        #region User Registration Logic
        /// <summary>
        /// Registers a new user and creates an associated authentication login account using a secure transaction.
        /// </summary>
        /// <param name="model">The registration payload details containing personal and account settings.</param>
        /// <returns>A tuple indicating success status and a feedback response message.</returns>
        public async Task<(bool Success, string Message)> RegisterNewUserAccountAsync(UserRegisterViewModel model)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var emailExists = await _context.Set<UserAccount>().AnyAsync(u => u.Email == model.Email);
                if (emailExists) return (false, "This email is already registered inside the system!");

                string oneTimePassword = Guid.NewGuid().ToString().Substring(0, 8);

                var newUser = new User
                {
                    Dept_Id = model.Dept_Id,
                    User_Name = model.User_Name,
                    Gender = model.Gender,
                    Nrc = model.Nrc,
                    Dob = DateTime.Parse(model.Dob),
                    Married_Status = model.Married_Status,
                    Position = model.Position,
                    Hired_Date = DateTime.Parse(model.Hired_Date),
                    Qualification = model.Qualification,
                    User_Ph_No = model.User_Ph_No,
                    Address = model.Address,
                    Is_Active = true,
                    Created_At = DateTime.UtcNow
                };

                _context.Set<User>().Add(newUser);
                await _context.SaveChangesAsync(); 

                var userAccount = new UserAccount
                {
                    User_Id = newUser.User_Id,
                    Role_Id = model.Role_Id == 0 ? (int)UserRole.Employee : model.Role_Id,
                    Email = model.Email,
                    Password_Hash = BCrypt.Net.BCrypt.HashPassword(oneTimePassword),
                    Is_First_Login = true,
                    Created_At = DateTime.UtcNow
                };

                _context.Set<UserAccount>().Add(userAccount);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                await _emailService.SendOneTimePasswordAsync(userAccount.Email, newUser.User_Name, oneTimePassword);

                return (true, "Account created successfully! An automated one-time password has been sent to the registered email.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                string detailedError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
 
                return (false, $"Database Error: {detailedError}");
            }
        }
        #endregion

        #region Directory Inquiry Lists
        /// <summary>
        /// Fetches a list of all HR profiles filtered by their active or resigned status state.
        /// </summary>
        /// <param name="status">The structural active or resigned status condition filter criteria.</param>
        /// <returns>A collection of HR records mapped to directory data transfer presentation schemas.</returns>
        public async Task<IEnumerable<HRDirectoryViewModel>> GetHRDirectoryListAsync(string status)
        {
            var query = from u in _context.Set<User>()
                        join ua in _context.Set<UserAccount>() on u.User_Id equals ua.User_Id
                        where ua.Role_Id == (int)UserRole.HR
                        select u;

            if (status == "Active")
            {
                query = query.Where(u => u.Is_Active == true);
            }
            else if (status == "Resigned")
            {
                query = query.Where(u => u.Is_Active == false);
            }

            return await query
                .Select(u => new HRDirectoryViewModel
                {
                    User_Id = u.User_Id,
                    HR_Id = $"HR-{u.User_Id:D2}",
                    HR_Name = u.User_Name,
                    Email = _context.Set<UserAccount>().Where(ua => ua.User_Id == u.User_Id).Select(ua => ua.Email).FirstOrDefault() ?? "N/A"
                }).ToListAsync();
        }

        /// <summary>
        /// Fetches a list of all standard employee profiles filtered by their active or resigned status state.
        /// </summary>
        /// <param name="status">The structural active or resigned status condition filter criteria.</param>
        /// <returns>A collection of employee records mapped to directory data transfer presentation schemas.</returns>
        public async Task<IEnumerable<HRDirectoryViewModel>> GetEmployeeDirectoryListAsync(string status)
        {
            var query = from u in _context.Set<User>()
                        join ua in _context.Set<UserAccount>() on u.User_Id equals ua.User_Id
                        where ua.Role_Id == (int)UserRole.Employee
                        select u;

            if (status == "Active")
            {
                query = query.Where(u => u.Is_Active == true);
            }
            else if (status == "Resigned")
            {
                query = query.Where(u => u.Is_Active == false);
            }

            return await query
                .Select(u => new HRDirectoryViewModel
                {
                    User_Id = u.User_Id,
                    HR_Id = $"EMP-{u.User_Id:D3}",
                    HR_Name = u.User_Name,
                    Email = _context.Set<UserAccount>().Where(ua => ua.User_Id == u.User_Id).Select(ua => ua.Email).FirstOrDefault() ?? "N/A"
                }).ToListAsync();
        }
        #endregion

        #region Profile Editing & Status Updates
        /// <summary>
        /// Fetches unified user profile records across relational tables to populate popup modification forms.
        /// </summary>
        /// <param name="userId">The unique profile identifier code integer to search.</param>
        /// <returns>An update display data payload view model schema, or null if records do not exist.</returns>
        public async Task<HRUpdateViewModel> GetHRForEditAsync(int userId)
        {
            var user = await _context.Set<User>()
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.User_Id == userId);

            if (user == null) return null;

            var userAccount = await _context.Set<UserAccount>()
                .FirstOrDefaultAsync(ua => ua.User_Id == userId);

            var resignation = await _context.Set<Resignation>()
                .FirstOrDefaultAsync(r => r.User_Id == userId);

            return new HRUpdateViewModel
            {
                User_Id = user.User_Id,
                Emp_Id = $"EMP-{user.User_Id:D3}",
                User_Name = user.User_Name,
                Gender = user.Gender,
                Dob = user.Dob,
                Nrc = user.Nrc,
                Married_Status = user.Married_Status,
                Position = user.Position,
                DepartmentName = user.Department?.DepartmentName ?? "No Department",
                Hired_Date = user.Hired_Date,
                Qualification = user.Qualification,
                User_Ph_No = user.User_Ph_No,
                Email = userAccount?.Email ?? "N/A",
                Address = user.Address,
                AccountStatus = user.Is_Active ? "Active" : "Resigned",
                ResignDateStr = resignation?.Resignation_Date.ToString("yyyy-MM-dd"),
                ResignReason = resignation?.Resignation_Reason
            };
        }

        /// <summary>
        /// Updates a user's employment status and creates, updates, or cleans up associated resignation table tracking metrics.
        /// </summary>
        /// <param name="model">The update configuration instructions data input metrics.</param>
        /// <param name="currentAdminId">The active system operator administrative actor identifier code.</param>
        /// <returns>True if transactional operations finish successfully, otherwise false.</returns>
        public async Task<bool> UpdateHRStatusAsync(HRUpdateViewModel model, int currentAdminId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _context.Set<User>().FirstOrDefaultAsync(u => u.User_Id == model.User_Id);
                if (user == null) return false;

                bool isFormActive = model.AccountStatus == "Active";
                user.Is_Active = isFormActive;
                user.updated_at = DateTime.UtcNow;
                _context.Set<User>().Update(user);

                if (!isFormActive)
                {
                    var existingResign = await _context.Set<Resignation>().FirstOrDefaultAsync(r => r.User_Id == model.User_Id);
                    DateTime resignDate = DateTime.TryParse(model.ResignDateStr, out var parsedDate) ? parsedDate : DateTime.UtcNow.Date;

                    if (existingResign == null)
                    {
                        var resignation = new Resignation
                        {
                            User_Id = model.User_Id,
                            Resignation_Date = resignDate,
                            Resignation_Reason = model.ResignReason ?? "No Reason provided",
                            Resigned_By_User_Id = currentAdminId,
                            Created_At = DateTime.UtcNow
                        };
                        _context.Set<Resignation>().Add(resignation);
                    }
                    else
                    {
                        existingResign.Resignation_Date = resignDate;
                        existingResign.Resignation_Reason = model.ResignReason ?? "No Reason provided";
                        existingResign.Resigned_By_User_Id = currentAdminId;
                        _context.Set<Resignation>().Update(existingResign);
                    }
                }
                else
                {
                    var existingResign = await _context.Set<Resignation>().FirstOrDefaultAsync(r => r.User_Id == model.User_Id);
                    if (existingResign != null)
                    {
                        _context.Set<Resignation>().Remove(existingResign);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();              
                return false;
            }
        }
        #endregion
    }
}