using System;
using System.ComponentModel.DataAnnotations;
using HRMS.Data.Entities;

namespace HRMS.Models.Holiday
{
    public class HolidayViewModel
    {
        [Required]
        public string Holiday_Name { get; set; }

        [Required]
        public string Holiday_Type { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Start_Date { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime End_Date { get; set; }

        public bool Is_Recurring { get; set; }

        public IEnumerable<PublicHoliday> Holidays { get; set; } = new List<PublicHoliday>();
    }
}