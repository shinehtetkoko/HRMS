using System.Collections.Generic;
namespace HRMS.Models.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalDepartments { get; set; }
        public int RegisteredHRsCount { get; set; }
        public int ActiveEmployees { get; set; }
        public string SystemHealth { get; set; } = "99.9% Up";

        public List<HRUserViewModel> RegisteredHRs { get; set; } = new();
    }

    public class HRUserViewModel
    {
        public string HRId { get; set; } = string.Empty;
        public string HRName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Status { get; set; }
        public bool Is_Active { get; set; }
    }
}