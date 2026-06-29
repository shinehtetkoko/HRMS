using System.ComponentModel.DataAnnotations;
using HRMS.Enums.Position;

namespace HRMS.Models.Employee
{
    public class EmployeeImportDtoViewModel
    {
        [Required] public string User_Name { get; set; }
        [Required] public string Department_Name { get; set; } 
        [Required] public int Gender { get; set; }
        [Required, StringLength(50)] public string Nrc { get; set; }
        [Required] public DateTime Dob { get; set; }
        [Required] public int Married_Status { get; set; }
        [Required] public Position Position { get; set; }
        [Required] public DateTime Hired_Date { get; set; }
        [Required, StringLength(50)] public string Qualification { get; set; }
        [Required, StringLength(20)] public string User_Ph_No { get; set; }
        [Required, StringLength(255)] public string Address { get; set; }
        [Required, EmailAddress] public string Email { get; set; }
        public int RowNumber { get; set; }
        public string ErrorMessage { get; set; }
    }
}