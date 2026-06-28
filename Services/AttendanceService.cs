using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
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
    }
}