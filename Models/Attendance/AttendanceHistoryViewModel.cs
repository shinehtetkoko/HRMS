using System.Collections.Generic;
using HRMS.Data.Entities;

namespace HRMS.Models.Attendance
{
    public class AttendanceHistoryViewModel
    {
        public IEnumerable<HRMS.Data.Entities.Attendance> Attendances { get; set; } = new List<HRMS.Data.Entities.Attendance>();
        public int SelectedMonth { get; set; }
        public int SelectedYear { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
        public int PageSize { get; set; } = 10;
    }
}
