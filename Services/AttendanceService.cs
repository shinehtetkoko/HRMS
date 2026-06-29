using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace HRMS.Services
{
    public class AttendanceService : IAttendanceService
    {
        private readonly AppDbContext _context;

        public AttendanceService(AppDbContext context)
        {
            _context = context;
        }

        #region Attendance Processing Logic
        /// <summary>
        /// Processes the daily check-in operation, handles file upload for selfies, evaluates late criteria, and logs the attendance record.
        /// </summary>
        /// <param name="userId">The unique identifier of the employee logging in.</param>
        /// <param name="workLocation">The type of work location (e.g., Office, Field).</param>
        /// <param name="checkInMode">The mode of check-in (e.g., Standard, Remote).</param>
        /// <param name="locationDetails">Optional details of the location.</param>
        /// <param name="attachment">The optional image file attachment uploaded by the user.</param>
        /// <returns>A tuple indicating whether the check-in succeeded along with a corresponding response message.</returns> 
        public async Task<(bool Success, string Message)> ProcessCheckInAsync(
            int userId, string workLocation, string checkInMode, string? locationDetails, IFormFile? attachment)
        {
            try
            {
                var todayUtc = DateTime.UtcNow.Date;

                var alreadyCheckedIn = await _context.Set<Attendance>()
                    .AnyAsync(a => a.User_Id == userId && a.Attendance_Date == todayUtc);

                if (alreadyCheckedIn)
                {
                    return (false, "You are already checked in for today!");
                }

                string? attachmentPath = null;
                if (attachment != null && attachment.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(attachment.FileName);
                    var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    var path = Path.Combine(uploadDir, fileName);
                    await using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await attachment.CopyToAsync(stream);
                        await stream.FlushAsync();
                    }
                    attachmentPath = "/uploads/" + fileName;
                }

                string attendanceStatus = "Present";
                var currentLocalTime = DateTime.Now;
                var lateCutoff = new TimeSpan(8, 15, 0);

                if (currentLocalTime.TimeOfDay > lateCutoff)
                {
                    attendanceStatus = "Late";
                }

                var attendanceLog = new Attendance
                {
                    User_Id = userId,
                    Attendance_Date = todayUtc,
                    Check_In = DateTime.UtcNow,
                    Attendance_Status = attendanceStatus,
                    Work_Location = workLocation,
                    Check_In_Mode = checkInMode,
                    Location_Detail = locationDetails,
                    Attachment = attachmentPath,
                    created_at = DateTime.UtcNow
                };

                _context.Set<Attendance>().Add(attendanceLog);
                await _context.SaveChangesAsync();

                return (true, "Daily check-in successful!");
            }
            catch (Exception ex)
            {
                return (false, "Unexpected error: " + ex.Message);
            }
        }

        /// <summary>
        /// Processes the daily check-out operation by stamping the current timestamp onto the existing check-in record.
        /// </summary>
        /// <param name="userId">The unique identifier of the employee logging out.</param>
        /// <returns>A tuple indicating whether the check-out succeeded along with a corresponding response message.</returns>
        public async Task<(bool Success, string Message)> ProcessCheckOutAsync(int userId)
        {
            try
            {
                var todayUtc = DateTime.UtcNow.Date;

                var attendanceRecord = await _context.Set<Attendance>()
                    .FirstOrDefaultAsync(a => a.User_Id == userId && a.Attendance_Date == todayUtc);

                if (attendanceRecord == null)
                {
                    return (false, "You haven't checked in for today yet!");
                }

                if (attendanceRecord.Check_Out != null)
                {
                    return (false, "You have already checked out for today!");
                }

                attendanceRecord.Check_Out = DateTime.UtcNow;

                _context.Set<Attendance>().Update(attendanceRecord);
                await _context.SaveChangesAsync();

                return (true, "Daily check-out successful!");
            }
            catch (Exception ex)
            {
                return (false, "Unexpected error during check-out: " + ex.Message);
            }
        }
        #endregion

        #region Attendance Inquiry & Reporting
        /// <summary>
        /// Fetches the descriptive relational attendance query logs for a user, filtered by target months and years.
        /// </summary>
        /// <param name="userId">The target user account identifier to inspect.</param>
        /// <param name="month">The exact month filter digit metrics.</param>
        /// <param name="year">The exact year filter digits context criteria.</param>
        /// <returns>A descending chronological order collection list of attendance records populated with user and department contexts.</returns>
        public async Task<List<Attendance>> GetAttendanceHistoryAsync(int userId, int month, int year)
        {
            return await _context.Set<Attendance>()
                .Include(a => a.User)
                    .ThenInclude(u => u.Department)
                .Where(a => a.User_Id == userId &&
                            a.Attendance_Date.Month == month &&
                            a.Attendance_Date.Year == year)
                .OrderByDescending(a => a.Attendance_Date)
                .ToListAsync();
        }
        #endregion

        #region Attendance Record
        /// <summary>
        /// Queries the database to retrieve a paged, filtered collection of attendance records for administration view, 
        /// along with supporting dropdown selection lists for departments and employees.
        /// </summary>
        /// <param name="month">The optional month filter constraint; defaults to the current calendar month if null.</param>
        /// <param name="year">The optional year filter constraint; defaults to the current calendar year if null.</param>
        /// <param name="dept">The selected department filter parameter used to target a dedicated corporate division.</param>
        /// <param name="employee">The selected employee username criteria utilized to isolate personal tracking profiles.</param>
        /// <param name="page">The current page segment index configuration targeted for index layout slicing.</param>
        /// <param name="pageSize">The maximum number of rows or record segments allowed per individual page index slice.</param>
        /// <returns>A structured attendance record view model filled with segmented log metrics and dashboard list states.</returns>
        public async Task<AttendanceRecordViewModel> GetAttendanceRecordsAsync(int? month, int? year, string? dept, string? employee, int page, int pageSize)
        {
            int searchMonth = month ?? DateTime.Now.Month;
            int searchYear = year ?? DateTime.Now.Year;

            string[] monthNames = { "", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            string monthText = (searchMonth >= 1 && searchMonth <= 12) ? monthNames[searchMonth] : DateTime.Now.ToString("MMMM");

            var allDepartments = await _context.Set<Attendance>()
                .Include(a => a.User)
                .ThenInclude(u => u.Department)
                .Where(a => a.User != null && a.User.Department != null)
                .Select(a => a.User.Department.DepartmentName)
                .Distinct()
                .OrderBy(d => d)
                .ToListAsync() ?? new List<string>();

            var allEmployees = await _context.Set<Attendance>()
                .Include(a => a.User)
                .Where(a => a.User != null)
                .Select(a => a.User.User_Name)
                .Distinct()
                .OrderBy(e => e)
                .ToListAsync() ?? new List<string>();

            var query = _context.Set<Attendance>()
                .Include(a => a.User)
                .ThenInclude(u => u.Department)
                .AsQueryable();

            query = query.Where(a => a.Attendance_Date.Month == searchMonth && a.Attendance_Date.Year == searchYear);

            if (!string.IsNullOrEmpty(dept) && dept != "Select Department")
            {
                query = query.Where(a => a.User != null && a.User.Department != null && a.User.Department.DepartmentName == dept);
            }

            if (!string.IsNullOrEmpty(employee) && employee != "Select Employee")
            {
                query = query.Where(a => a.User != null && a.User.User_Name == employee);
            }

            int totalRecords = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            if (totalPages < 1) totalPages = 1;

            var records = await query
                .OrderByDescending(a => a.Attendance_Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync() ?? new List<Attendance>();

            return new AttendanceRecordViewModel
            {
                SelectedMonth = searchMonth,
                DisplayMonthName = monthText,
                SelectedYear = searchYear,
                SelectedDept = dept ?? "Select Department",
                SelectedEmployee = employee ?? "Select Employee",
                CurrentPage = page,
                TotalPages = totalPages,
                TotalRecords = totalRecords,
                PageSize = pageSize,
                Departments = allDepartments,
                Employees = allEmployees,
                Attendances = records.Select(item => new AttendanceItemViewModel
                {
                    User_Id = item.User_Id,
                    Attendance_Date = item.Attendance_Date,
                    Check_In = item.Check_In,
                    Check_Out = item.Check_Out,
                    Attendance_Status = item.Attendance_Status ?? "Present",
                    User = new UserViewModel
                    {
                        User_Name = item.User?.User_Name ?? "-",
                        Department = new DepartmentViewModel
                        {
                            DepartmentName = item.User?.Department?.DepartmentName ?? "-"
                        }
                    }
                }).ToList()
            };
        }

        /// <summary>
        /// Compiles the filtered database logs into a structured text data stream, appending a UTF-8 signature for spreadsheet compatibility.
        /// </summary>
        /// <param name="month">The calendar month criteria mapped to bound target record extractions.</param>
        /// <param name="year">The calendar year criteria mapped to bound target record extractions.</param>
        /// <param name="dept">The corporate department query filter used to scope records by division.</param>
        /// <param name="employee">The employee identification name handle utilized to scope log lists.</param>
        /// <returns>A continuous binary array stream representation containing raw comma-separated spreadsheet data sequences.</returns>
        public async Task<byte[]> ExportAttendanceRecordsAsync(int? month, int? year, string? dept, string? employee)
        {
            int searchMonth = month ?? DateTime.Now.Month;
            int searchYear = year ?? DateTime.Now.Year;

            var query = _context.Set<Attendance>()
                .Include(a => a.User)
                .ThenInclude(u => u.Department)
                .AsQueryable();

            query = query.Where(a => a.Attendance_Date.Month == searchMonth && a.Attendance_Date.Year == searchYear);

            if (!string.IsNullOrEmpty(dept) && dept != "Select Department")
            {
                query = query.Where(a => a.User.Department.DepartmentName == dept);
            }

            if (!string.IsNullOrEmpty(employee) && employee != "Select Employee")
            {
                query = query.Where(a => a.User.User_Name == employee);
            }

            var records = await query.OrderByDescending(a => a.Attendance_Date).ToListAsync();

            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Employee ID,Employee Name,Department,Date,Check-in,Check-out,Status,Total Work Hours");

            foreach (var item in records)
            {
                var userName = item.User?.User_Name ?? "-";
                var deptName = item.User?.Department?.DepartmentName ?? "-";
                var dateStr = item.Attendance_Date.ToString("dd MMM yyyy");
                var checkIn = item.Check_In.ToLocalTime().ToString("hh:mm tt");
                var checkOut = item.Check_Out.HasValue ? item.Check_Out.Value.ToLocalTime().ToString("hh:mm tt") : "-";

                var hours = "0h 00m";
                if (item.Check_Out.HasValue)
                {
                    var duration = item.Check_Out.Value - item.Check_In;
                    hours = $"{duration.Hours}h {duration.Minutes:D2}m";
                }

                builder.AppendLine($"EMP_{item.User_Id},{userName},{deptName},{dateStr},{checkIn},{checkOut},{item.Attendance_Status},{hours}");
            }

            var preamble = System.Text.Encoding.UTF8.GetPreamble();
            var bytes = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            return preamble.Concat(bytes).ToArray();
        }
        #endregion
    }
}