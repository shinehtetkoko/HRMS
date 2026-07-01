namespace HRMS.Models.Employee
{
    public class DirectoryViewModel
    {
        public IEnumerable<HRMS.Models.Admin.HRDirectoryViewModel> HRDirectoryList { get; set; }
        public IEnumerable<HRMS.Models.Employee.EmployeeDirectoryViewModel> EmployeeDirectoryList { get; set; }

    }
}