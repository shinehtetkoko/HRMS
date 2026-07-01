namespace HRMS.Models.Employee
{
    public class HRDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int TotalDepartments { get; set; }
        public int TotalOnLeaveToday { get; set; }
        public int TotalPendingActions { get; set; } 
        public int PendingLeaveRequests { get; set; }
        public int PendingProfileRequests { get; set; }   
    }
}