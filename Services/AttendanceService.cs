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

        // Daily Check-In Logic 
        public async Task<(bool Success, string Message)> ProcessCheckInAsync(
            int userId, string workLocation, string checkInMode, string? locationDetails, IFormFile? attachment)
        {
            try
            {
                var todayUtc = DateTime.UtcNow.Date;

                // Check already checked in
                var alreadyCheckedIn = await _context.Set<Attendance>()
                    .AnyAsync(a => a.User_Id == userId && a.Attendance_Date == todayUtc);

                if (alreadyCheckedIn)
                {
                    return (false, "You are already checked in for today!");
                }

                // File Attachment (Selfie) Logic
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

                // Late Check-in status logic
                string attendanceStatus = "Present";
                var currentLocalTime = DateTime.Now;
                var lateCutoff = new TimeSpan(8, 15, 0);

                if (currentLocalTime.TimeOfDay > lateCutoff)
                {
                    attendanceStatus = "Late";
                }

                // Insert into DB Entity
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

        // Daily Check-Out Logic

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

                // Check-Out time
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


        // Attendance History 
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
    }
}