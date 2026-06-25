using System;
using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Admin
{
    public class CompanyProfileViewModel
    {
        public int Comp_Id { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(100, ErrorMessage = "Company Name cannot exceed 100 characters")]
        public string Comp_Name { get; set; }

        [Required(ErrorMessage = "Company Phone Number is required")]
        [StringLength(20)]
        public string Comp_Ph_No { get; set; }

        [Required(ErrorMessage = "Company Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [StringLength(50)]
        public string Comp_Email { get; set; }

        [Required(ErrorMessage = "Company Location is required")]
        [StringLength(255)]
        public string Comp_Location { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(500)]
        public string Description { get; set; }

        [Required(ErrorMessage = "Office Start Time is required")]
        public TimeSpan Office_Start_Time { get; set; }

        [Required(ErrorMessage = "Office End Time is required")]
        public TimeSpan Office_End_Time { get; set; }
    }
}