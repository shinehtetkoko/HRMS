using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Employee
{
    public class UserRegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50)]
        public string User_Name { get; set; }

        [Required(ErrorMessage = "Department is required")]
        public int Dept_Id { get; set; }

        [Required(ErrorMessage = "Gender is required")]
        public int Gender { get; set; }

        [Required(ErrorMessage = "NRC is required")]
        [StringLength(50)]
        public string Nrc { get; set; }

        [Required(ErrorMessage = "Date of Birth is required")]
        public string Dob { get; set; }

        [Required(ErrorMessage = "Married Status is required")]
        public int Married_Status { get; set; }

        [Required(ErrorMessage = "Position is required")]
        [StringLength(50)]
        public string Position { get; set; }

        [Required(ErrorMessage = "Hired Date is required")]
        public string Hired_Date { get; set; }

        [Required(ErrorMessage = "Qualification is required")]
        [StringLength(50)]
        public string Qualification { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(20)]
        public string User_Ph_No { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(255)]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(50)]
        public string Email { get; set; }

        [Required]
        public int Role_Id { get; set; } 
    }
}
