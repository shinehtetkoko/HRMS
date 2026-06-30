using HRMS.Data.Entities;
using HRMS.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IAttendanceService
    {
        Task<(bool Success, string Message)> ProcessCheckInAsync(
            int userId,
            string workLocation,
            string checkInMode,
            string? locationDetails,
            IFormFile? attachment);

        Task<List<Attendance>> GetAttendanceHistoryAsync(int userId, int month, int year);
        Task<(bool Success, string Message)> ProcessCheckOutAsync(int userId);
        Task<AttendanceRecordViewModel> GetAttendanceRecordsAsync(int? month, int? year, string? dept, string? employee, int page, int pageSize);
        Task<byte[]> ExportAttendanceRecordsAsync(int? month, int? year, string? dept, string? employee);
    }
}