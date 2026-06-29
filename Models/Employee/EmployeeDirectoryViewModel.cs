namespace HRMS.Models.Employee
{
    public class EmployeeDirectoryViewModel
    {
        public int UserId { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentName { get; set; }
        public bool HasPendingRequest { get; set; }

        public string CurrentPhoneNumber { get; set; }
        public string CurrentAddress { get; set; }

        public string NewPhoneNumber { get; set; }
        public string NewAddress { get; set; }
    }
}