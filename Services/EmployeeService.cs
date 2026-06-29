using BCrypt.Net;
using OfficeOpenXml;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
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
using HRMS.Enums.Position;

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

        #region Profile
        /// <summary>
        /// Retrieves the profile data for a specific user by their user ID.
        /// </summary>
        /// <param name="userId">The logged in employee ID.</param>
        /// <returns>The view for the Profile.</returns>
        public async Task<ProfileViewModel> GetProfileDataAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.User_Id == userId);

            if (user == null) return null;

            var userAccount = await _context.UserAccounts
                .FirstOrDefaultAsync(ua => ua.User_Id == userId);

            return new ProfileViewModel
            {
                EmployeeId = $"EMP{user.User_Id:D3}",
                Name = user.User_Name,
                Gender = user.Gender == 1 ? "Male" : "Female",
                DOB = user.Dob.ToString("dd / MM / yyyy"),
                NRC = user.Nrc,
                MarriedStatus = user.Married_Status == 1 ? "Married" : "Single",
                Position = user.Position.ToString(),
                Department = user.Department?.DepartmentName ?? "N/A",
                HiredDate = user.Hired_Date.ToString("dd / MM / yyyy"),
                Qualification = user.Qualification,
                PhoneNumber = user.User_Ph_No,
                Email = userAccount?.Email ?? "N/A",
                Address = user.Address
            };
        }

        /// <summary>
        /// Submits a profile update request.
        /// </summary>
        /// <param name="request">The view model containing the proposed profile changes.</param>
        /// <returns>True if the request submitted sucessfully.</returns>
        public async Task<bool> SubmitProfileUpdateRequestAsync(UpdateProfileRequestViewModel request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPhoneNumber) && string.IsNullOrWhiteSpace(request.NewAddress))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(request.NewPhoneNumber))
            {
                var phoneRegex = new Regex(@"^09[0-9\-]{7,10}$");
                if (!phoneRegex.IsMatch(request.NewPhoneNumber.Trim()))
                {
                    return false;
                }
            }

            var updateRequest = new ProfileUpdateRequest
            {
                UserId = request.UserId,
                NewPhoneNumber = request.NewPhoneNumber?.Trim(),
                NewAddress = request.NewAddress?.Trim(),
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };
            _context.ProfileUpdateRequests.Add(updateRequest);
            await _context.SaveChangesAsync();

            return true;
        }
        #endregion

        #region EmployeeDirectory
        // <summary>
        /// Retrieves a list of registered departments.
        /// </summary>
        /// <returns>Collection of departments.</returns>
        public async Task<IEnumerable<string>> GetDepartmentNamesAsync()
        {
            return await _context.Departments
                                 .Select(d => d.DepartmentName)
                                 .Distinct()
                                 .ToListAsync();
        }

        // <summary>
        /// Retrieves filtered employee list.
        /// </summary>
        /// <param name="status">The employment status filer("Active","Resigned","All")</param>
        /// <param name="department">The department name filter.</param>
        /// <param name="page">The current page number for pagination.</param>
        /// <param name="pageSize">The number of employees return per page.</param>
        /// <returns>Paginated list of employee records.</returns>
        public async Task<PagedResultViewModel<EmployeeDirectoryViewModel>> GetFilteredEmployeesAsync(string status, string department, int page, int pageSize)
        {
            bool? isActiveFilter = null;
            if (status == "Active") isActiveFilter = true;
            else if (status == "Resigned") isActiveFilter = false;

            var query = _context.Users
                .Include(u => u.Department)
                .Where(u => _context.UserAccounts.Any(ua => ua.User_Id == u.User_Id && ua.Role_Id == 3));

            if (isActiveFilter.HasValue)
            {
                query = query.Where(u => u.Is_Active == isActiveFilter.Value);
            }

            if (!string.IsNullOrEmpty(department) && department != "All")
            {
                query = query.Where(u => u.Department != null && u.Department.DepartmentName == department);
            }

            int totalRecords = await query.CountAsync();

            var projectedQuery = query.Select(u => new EmployeeDirectoryViewModel
            {
                UserId = u.User_Id,
                EmployeeName = u.User_Name,
                DepartmentName = u.Department != null ? u.Department.DepartmentName : "N/A",
                CurrentPhoneNumber = u.User_Ph_No,
                CurrentAddress = u.Address,
                HasPendingRequest = _context.ProfileUpdateRequests.Any(r => r.UserId == u.User_Id && r.Status == "Pending"),
                NewPhoneNumber = _context.ProfileUpdateRequests
                    .Where(r => r.UserId == u.User_Id && r.Status == "Pending")
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.NewPhoneNumber)
                    .FirstOrDefault() ?? u.User_Ph_No,
                NewAddress = _context.ProfileUpdateRequests
                    .Where(r => r.UserId == u.User_Id && r.Status == "Pending")
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => r.NewAddress)
                    .FirstOrDefault() ?? u.Address
            });

            var orderedQuery = projectedQuery
                .OrderByDescending(e => e.HasPendingRequest)
                .ThenBy(e => e.EmployeeName);

            var pagedItems = await orderedQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResultViewModel<EmployeeDirectoryViewModel>
            {
                Items = pagedItems,
                CurrentPage = page,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };

        }

        /// <summary>
        /// Export to Excel containing a filtered list of employees.
        /// </summary>
        /// <param name="status">The employment status filer("Active","Resigned","All")</param>
        /// <param name="department">The department name filter.</param>
        /// <returns>A byte array representing the generated Excel workbook data.</returns>
        public async Task<byte[]> ExportEmployeesToExcelAsync(string status, string department)
        {
            var pagedResult = await GetFilteredEmployeesAsync(status, department, page: 1, pageSize: 99999);
            var employees = pagedResult.Items;

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Employee Directory");

                worksheet.Cell(1, 1).Value = "Employee ID";
                worksheet.Cell(1, 2).Value = "Employee Name";
                worksheet.Cell(1, 3).Value = "Department";
                worksheet.Cell(1, 4).Value = "Has Pending Request";

                var headerRange = worksheet.Range("A1:D1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var emp in employees)
                {
                    worksheet.Cell(row, 1).Value = "EMP" + emp.UserId.ToString("D3");
                    worksheet.Cell(row, 2).Value = emp.EmployeeName;
                    worksheet.Cell(row, 3).Value = emp.DepartmentName;
                    worksheet.Cell(row, 4).Value = emp.HasPendingRequest ? "Yes" : "No";
                    row++;
                }
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        /// <summary>
        /// Read and validate uploaded excel file.
        /// </summary>
        /// <param name="file">The uploaded excel file.</param>
        /// <returns>A list of tuple with descripted messages.</returns>
        public async Task<(List<EmployeeImportDtoViewModel> ValidData, List<EmployeeImportDtoViewModel> Errors)> ReadExcelAsync(IFormFile file)
        {
            var validList = new List<EmployeeImportDtoViewModel>();
            var errorRows = new List<EmployeeImportDtoViewModel>();

            ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        var currentErrorMessages = new List<string>();
                        string nameFromExcel = worksheet.Cells[row, 1].Value?.ToString();
                        // Validation logic
                        bool hasEmptyColumn = false;
                        for (int col = 1; col <= 12; col++)
                        {
                            if (worksheet.Cells[row, col].Value == null || string.IsNullOrWhiteSpace(worksheet.Cells[row, col].Value.ToString()))
                            {
                                hasEmptyColumn = true;
                                break;
                            }
                        }

                        if (hasEmptyColumn)
                        {
                            currentErrorMessages.Add("Missing required columns");
                        }
                        if (string.IsNullOrEmpty(nameFromExcel))
                        {
                            currentErrorMessages.Add("Name is missing");
                        }
                        else if (!Regex.IsMatch(nameFromExcel, @"^[a-zA-Z\s]+$"))
                        {
                            currentErrorMessages.Add("Name should not contain special characters");
                        }
                        string posString = worksheet.Cells[row, 7].Value?.ToString();

                        if (!Enum.TryParse(posString, out Position pos))
                        {
                            currentErrorMessages.Add("Invalid Position: " + posString);
                        }
                        if (currentErrorMessages.Any())
                        {
                            errorRows.Add(new EmployeeImportDtoViewModel
                            {
                                RowNumber = row,
                            });
                        }
                        var dto = new EmployeeImportDtoViewModel
                        {
                            RowNumber = row,
                            User_Name = worksheet.Cells[row, 1].Value?.ToString(),
                            Department_Name = worksheet.Cells[row, 2].Value?.ToString(),
                            Gender = int.TryParse(worksheet.Cells[row, 3].Value?.ToString(), out int g) ? g : 0,
                            Nrc = worksheet.Cells[row, 4].Value?.ToString(),
                            Dob = DateTime.TryParse(worksheet.Cells[row, 5].Value?.ToString(), out DateTime d) ? d : DateTime.MinValue,
                            Married_Status = int.TryParse(worksheet.Cells[row, 6].Value?.ToString(), out int s) ? s : 0,
                            Position = pos,
                            Hired_Date = DateTime.TryParse(worksheet.Cells[row, 8].Value?.ToString(), out DateTime h) ? h : DateTime.MinValue,
                            Qualification = worksheet.Cells[row, 9].Value?.ToString(),
                            User_Ph_No = worksheet.Cells[row, 10].Value?.ToString(),
                            Address = worksheet.Cells[row, 11].Value?.ToString(),
                            Email = worksheet.Cells[row, 12].Value?.ToString()
                        };
                        validList.Add(dto);
                    }
                }
            }
            return (validList, errorRows);
        }

        /// <summary>
        /// Validates and imports a collection of employees.
        /// </summary>
        /// <param name="dtos"></param>
        /// <returns>A tuple containing a success flag and a list of failed rows with error descriptions.</returns>
        public async Task<(bool Success, List<EmployeeImportDtoViewModel> Errors)> ImportEmployeesFromExcel(List<EmployeeImportDtoViewModel> dtos)
        {
            var errorList = new List<EmployeeImportDtoViewModel>();
            var employeeRoleId = _context.Roles.FirstOrDefault(r => r.Role_Name == "Employee")?.Role_Id;
            var departments = _context.Departments.ToList();

            var existingNrcs = await _context.Users.Select(u => u.Nrc).ToListAsync();
            var existingPhones = await _context.Users.Select(u => u.User_Ph_No).ToListAsync();
            var existingEmails = await _context.UserAccounts.Select(ua => ua.Email).ToListAsync();

            bool importedAtLeastOne = false;
            int index = 0;
            dtos = dtos.OrderBy(d => d.RowNumber).ToList();
            foreach (var dto in dtos)
            {
                index++;
                //dto.RowNumber = index + 1;
                string displayName = string.IsNullOrEmpty(dto.User_Name) ? $"Row {dto.RowNumber}" : dto.User_Name;
                List<string> errors = new List<string>();
                //if (string.IsNullOrEmpty(dto.User_Name)) errors.Add("Name is missing");
                //if (!string.IsNullOrEmpty(dto.User_Name) && !Regex.IsMatch(dto.User_Name, @"^[a-zA-Z\s]+$"))
                //{
                //    errors.Add("Name should not contain special characters");
                //}
                var dept = departments.FirstOrDefault(d => d.DepartmentName == dto.Department_Name);

                if (dept == null) errors.Add("Department not found");


                if (!string.IsNullOrEmpty(dto.User_Ph_No) && !dto.User_Ph_No.All(char.IsDigit))
                {
                    errors.Add("Invalid Phone Number");
                }

                if (!string.IsNullOrEmpty(dto.Email) && !dto.Email.Contains("@"))
                {
                    errors.Add("Invalid Email format");
                }
                if (!Enum.IsDefined(typeof(Position), dto.Position))
                {
                    errors.Add("Invalid Position");
                }

                if (dto.Dob == DateTime.MinValue || dto.Hired_Date == DateTime.MinValue) errors.Add("Invalid Date");
                if (existingNrcs.Contains(dto.Nrc)) errors.Add("NRC already exists");
                if (existingPhones.Contains(dto.User_Ph_No)) errors.Add("Phone already exists");
                if (existingEmails.Contains(dto.Email)) errors.Add("Email already exists");

                if (errors.Count > 0)
                {
                    var existingError = errorList.FirstOrDefault(e => e.RowNumber == dto.RowNumber);
                    if (existingError != null)
                    {
                        existingError.ErrorMessage += ", " + string.Join(", ", errors);
                    }
                    else
                    {
                        dto.ErrorMessage = string.Join(", ", errors);
                        errorList.Add(dto);
                    }
                    continue;
                }

                else
                {
                    var user = new User
                    {
                        User_Name = dto.User_Name,
                        Dept_Id = dept.DepartmentId,
                        Gender = dto.Gender,
                        Nrc = dto.Nrc,
                        Dob = dto.Dob,
                        Married_Status = dto.Married_Status,
                        Position = dto.Position.ToString(),
                        Hired_Date = dto.Hired_Date,
                        Qualification = dto.Qualification,
                        User_Ph_No = dto.User_Ph_No,
                        Address = dto.Address,
                        Is_Active = true,
                        Created_At = DateTime.UtcNow
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                    string tempPassword = Guid.NewGuid().ToString().Substring(0, 8);
                    var account = new UserAccount
                    {
                        User_Id = user.User_Id,
                        Role_Id = employeeRoleId.Value,
                        Email = dto.Email,
                        Password_Hash = BCrypt.Net.BCrypt.HashPassword(tempPassword),
                        Is_First_Login = true
                    };

                    _context.UserAccounts.Add(account);
                    await _context.SaveChangesAsync();

                    existingNrcs.Add(dto.Nrc);
                    existingPhones.Add(dto.User_Ph_No);
                    existingEmails.Add(dto.Email);
                    importedAtLeastOne = true;

                    _ = Task.Run(() => _emailService.SendOneTimePasswordAsync(dto.Email, "Your Temporary Password", $"Hello {dto.User_Name}, your temporary password is: {tempPassword}"));
                }


            }

            return (importedAtLeastOne, errorList.OrderBy(e => e.RowNumber).ToList());
        }

        public MemoryStream GenerateErrorExcel(List<EmployeeImportDtoViewModel> errors)
        {
            var consolidatedErrors = errors.GroupBy(e => e.RowNumber)
                                   .Select(g => new {
                                       RowNumber = g.Key,
                                       ErrorMessage = string.Join(", ", g.Select(e => e.ErrorMessage))
                                   }).OrderBy(e => e.RowNumber).ToList();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var stream = new MemoryStream();
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets.Add("Errors");

                worksheet.Cells[1, 1].Value = "Row Number";
                worksheet.Cells[1, 2].Value = "Error Message";
                var sortedErrors = errors.OrderBy(e => e.RowNumber).ToList();
                
                for (int i = 0; i < consolidatedErrors.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = consolidatedErrors[i].RowNumber;
                    //worksheet.Cells[i + 2, 2].Value = string.IsNullOrEmpty(errors[i].User_Name) ? "N/A" : errors[i].User_Name;
                    worksheet.Cells[i + 2, 2].Value = consolidatedErrors[i].ErrorMessage;
                }

                worksheet.Cells.AutoFitColumns();
                package.Save();
            }
            stream.Position = 0;
            return stream;
        }

        /// <summary>
        /// Approve Profile Update Request.
        /// </summary>
        /// <param name="userId">The ID of the user whose profile is being updated.</param>
        /// <param name="reviewedByUserId">The ID of the HR manager reviewing the request.</param>
        /// <returns>True if updates were successfully applied.</returns>
        public async Task<bool> ApproveProfileUpdateAsync(int userId, int reviewedByUserId)
        {
            var pendingRequests = await _context.ProfileUpdateRequests
                .Where(r => r.UserId == userId && r.Status == "Pending")
                .ToListAsync();

            if (!pendingRequests.Any()) return false;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.User_Id == userId);
            if (user == null) return false;
            var latestRequest = pendingRequests.OrderByDescending(r => r.CreatedAt).First();
            if (!string.IsNullOrEmpty(latestRequest.NewPhoneNumber))
            {
                user.User_Ph_No = latestRequest.NewPhoneNumber;
            }
            if (!string.IsNullOrEmpty(latestRequest.NewAddress))
            {
                user.Address = latestRequest.NewAddress;
            }
            user.updated_at = DateTime.UtcNow;
            foreach (var req in pendingRequests)
            {
                req.Status = "Approved";
                req.ReviewedByUserId = reviewedByUserId;
                req.ReviewedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        #endregion
    }
}