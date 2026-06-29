using Microsoft.AspNetCore.Http;
using System;

namespace HRMS.Models
{
    /// <summary>
    /// Represents the view model for creating or updating a leave request.
    /// </summary>
    public class LeaveRequestViewModel
    {
        /// <summary> Gets or sets the unique identifier for the leave request. </summary>
        public int LeaveRequestId { get; set; }

        /// <summary> Gets or sets the ID of the selected leave type. </summary>
        public int LeaveTypeId { get; set; }

        /// <summary> Gets or sets the start date of the requested leave. </summary>
        public DateTime StartDate { get; set; }

        /// <summary> Gets or sets the end date of the requested leave. </summary>
        public DateTime EndDate { get; set; }

        /// <summary> Gets or sets the total number of days for the leave request. </summary>
        public int TotalDays { get; set; }

        /// <summary> Gets or sets the reason for the leave request. </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary> Gets or sets the optional file attachment for supporting documents. </summary>
        public IFormFile? Attachment { get; set; }
    }
}