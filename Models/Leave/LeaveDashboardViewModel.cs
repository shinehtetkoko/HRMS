namespace HRMS.Models
{
    /// <summary>
    /// Represents the view model for the leave dashboard, containing both balance information and request details.
    /// </summary>
    public class LeaveDashboardViewModel
    {
        /// <summary> Gets or sets the name of the leave type. </summary>
        public string LeaveName { get; set; }

        /// <summary> Gets or sets the number of remaining leave days. </summary>
        public int RemainingDays { get; set; }

        /// <summary> Gets or sets the total allocated leave days. </summary>
        public int TotalDays { get; set; }

        /// <summary> Gets or sets the file path or name of the attachment, if any. </summary>
        public string? Attachment { get; set; }

        /// <summary> Gets or sets the unique identifier for the leave request. </summary>
        public int LeaveRequestId { get; set; }

        /// <summary> Gets or sets the name of the employee. </summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary> Gets or sets the name of the employee's department. </summary>
        public string DepartmentName { get; set; } = string.Empty;

        /// <summary> Gets or sets the type of leave (e.g., Annual, Sick). </summary>
        public string LeaveType { get; set; } = string.Empty;

        /// <summary> Gets or sets the start date of the leave. </summary>
        public DateTime StartDate { get; set; }

        /// <summary> Gets or sets the end date of the leave. </summary>
        public DateTime EndDate { get; set; }

        /// <summary> Gets or sets the current status of the leave request (e.g., Pending, Approved). </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary> Gets or sets the reason provided for the leave. </summary>
        public string Reason { get; set; } = string.Empty;

        /// <summary> Gets or sets the unique identifier for the employee. </summary>
        public int EmployeeId { get; set; }
    }
}