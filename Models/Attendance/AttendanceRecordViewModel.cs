using System;
using System.Collections.Generic;

namespace HRMS.Models
{
    public class AttendanceRecordViewModel
    {
        public int SelectedMonth { get; set; } = DateTime.Now.Month;
        public string DisplayMonthName { get; set; } = DateTime.Now.ToString("MMMM");
        public int SelectedYear { get; set; } = DateTime.Now.Year;
        public string SelectedDept { get; set; } = "Select Department";
        public string SelectedEmployee { get; set; } = "Select Employee";
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 10;
        public IEnumerable<AttendanceItemViewModel> Attendances { get; set; } = new List<AttendanceItemViewModel>();
        public List<string> Departments { get; set; } = new List<string>();
        public List<string> Employees { get; set; } = new List<string>();
    }

    public class AttendanceItemViewModel
    {
        public int User_Id { get; set; }
        public UserViewModel? User { get; set; }
        public DateTime Attendance_Date { get; set; }
        public DateTime Check_In { get; set; }
        public DateTime? Check_Out { get; set; }
        public string Attendance_Status { get; set; } = string.Empty;
    }

    public class UserViewModel
    {
        public string User_Name { get; set; } = string.Empty;
        public DepartmentViewModel? Department { get; set; }
    }

    public class DepartmentViewModel
    {
        public string DepartmentName { get; set; } = string.Empty;
    }
}
