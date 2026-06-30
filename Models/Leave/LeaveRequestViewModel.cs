using System.Collections.Generic;
namespace HRMS.Models
{
    public class LeaveRequestViewModel
    {
        internal int TotalDays;

        public int LeaveRequestId { get; set; }
        public int User_Id { get; set; }
        public string User_Name { get; set; }
        public string Department { get; set; }
        public string LeaveType { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string DateRange { get; set; } 
        public string Status { get; set; }
        public string Reason { get; set; }
        public string Attachment { get; set; }
    }
}