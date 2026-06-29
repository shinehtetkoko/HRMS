using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace HRMS.Models.Employee 
{
    public class UpdateProfileRequestViewModel
    {
        public int UserId { get; set; }
        public string? NewPhoneNumber { get; set; }
        public string? NewAddress { get; set; }
    }
}