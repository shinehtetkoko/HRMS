using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Employee
{
    public class HRUpdateViewModel
    {
        [Required]
        public int User_Id { get; set; }

        public string? Emp_Id { get; set; }

        public string User_Name { get; set; } = string.Empty;

        public int Gender { get; set; }

        public DateTime Dob { get; set; }

        public string Nrc { get; set; } = string.Empty;

        public int Married_Status { get; set; }

        public string Position { get; set; } = string.Empty;

        public string DepartmentName { get; set; } = string.Empty;

        public DateTime Hired_Date { get; set; }

        public string Qualification { get; set; } = string.Empty;

        public string User_Ph_No { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        [Required]
        public string AccountStatus { get; set; } = "Active";

        public string? ResignDateStr { get; set; }

        public string? ResignReason { get; set; }
    }
}
