using HRMS.Data.Entities;
using System.Collections.Generic;

namespace HRMS.Models
{
    /// <summary>
    /// Represents the view model for the leave setup configuration page, 
    /// containing existing policies and types along with form fields for new entries.
    /// </summary>
    public class LeaveSetupViewModel
    {
        /// <summary> Gets or sets the list of existing leave policies. </summary>
        public IEnumerable<LeavePolicy> LeavePolicies { get; set; } = new List<LeavePolicy>();

        /// <summary> Gets or sets the list of available leave types. </summary>
        public IEnumerable<LeaveType> LeaveTypes { get; set; } = new List<LeaveType>();

        /// <summary> Gets or sets the ID of the selected leave type. </summary>
        public int Leave_Type_Id { get; set; }

        /// <summary> Gets or sets the total allocated days for the leave type. </summary>
        public int Total_Days { get; set; }

        /// <summary> Gets or sets a value indicating whether unused leave can be carried forward to the next year. </summary>
        public bool Carry_Forward { get; set; }

        /// <summary> Gets or sets the maximum number of days that can be carried forward, if applicable. </summary>
        public int? Max_Carry_Day { get; set; }

        /// <summary> Gets or sets the descriptive name of the leave type. </summary>
        public string Leave_Name { get; set; } = string.Empty;
    }
}