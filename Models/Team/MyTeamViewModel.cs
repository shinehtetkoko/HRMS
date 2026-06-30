using System;
using System.Collections.Generic;

namespace HRMS.Models
{
    public class MyTeamViewModel
    {
        public int PendingLeaveCount { get; set; }
        public int TeamMembersCount { get; set; }
        public int OnLeaveTodayCount { get; set; }
        public List<EmployeeDto> Employees { get; set; } = new List<EmployeeDto>();
        public string SelectedStatus { get; set; } = "All";
        public UserEditDto? SelectedEmployee { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class EmployeeDto
    {
        public string EmployeeId { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }

    public class UserEditDto
    {
        public int User_Id { get; set; }
        public string User_Name { get; set; } = string.Empty;
        public int Gender { get; set; }
        public DateTime? Dob { get; set; }
        public string Nrc { get; set; } = string.Empty;
        public int Married_Status { get; set; }
        public string Position { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public DateTime? Hired_Date { get; set; }
        public string Qualification { get; set; } = string.Empty;
        public string User_Ph_No { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
    }
}
