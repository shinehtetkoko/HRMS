using Microsoft.AspNetCore.Http;
using System;

namespace HRMS.Models
{
    /// <summary>
    /// Represents the view model for creating, updating, and displaying a leave request.
    /// </summary>
    public class LeaveRequestViewModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the leave request.
        /// </summary>
        public int LeaveRequestId { get; set; }

        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        public int User_Id { get; set; }

        /// <summary>
        /// Gets or sets the user's name.
        /// </summary>
        public string? User_Name { get; set; }

        /// <summary>
        /// Gets or sets the department name.
        /// </summary>
        public string? Department { get; set; }

        /// <summary>
        /// Gets or sets the leave type name.
        /// </summary>
        public string? LeaveType { get; set; }

        /// <summary>
        /// Gets or sets the leave type ID.
        /// </summary>
        public int LeaveTypeId { get; set; }

        /// <summary>
        /// Gets or sets the start date of the leave.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the end date of the leave.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Gets or sets the formatted date range for display.
        /// </summary>
        public string? DateRange { get; set; }

        /// <summary>
        /// Gets or sets the total number of leave days.
        /// </summary>
        public int TotalDays { get; set; }

        /// <summary>
        /// Gets or sets the leave request status.
        /// </summary>
        public string? Status { get; set; }

        /// <summary>
        /// Gets or sets the reason for the leave request.
        /// </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the uploaded attachment.
        /// </summary>
        public IFormFile? Attachment { get; set; }

        /// <summary>
        /// Gets or sets the saved attachment path.
        /// </summary>
        public string? AttachmentPath { get; set; }
    }
}