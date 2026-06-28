using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace HRMS.Models.Attendance
{
    public class DailyCheckInViewModel
    {
        [Required(ErrorMessage = "Work location is required.")]
        public string WorkLocation { get; set; } = string.Empty;

        [Required(ErrorMessage = "Check-in mode is required.")]
        public string CheckInMode { get; set; } = string.Empty;

        public string? LocationDetails { get; set; }

        public IFormFile? Attachment { get; set; }
    }
}
