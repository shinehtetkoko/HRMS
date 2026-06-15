using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities { 
    public class LeaveRequest
    {
        [Key]
        [Column("leave_request_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Leave_Request_Id { get; set; }

        [Column("user_id")]
        [Required] 
        public int User_Id { get; set; }

        [ForeignKey(nameof(User_Id))]
        public User User { get; set; }

        [Column("leave_type_id")]
        [Required] 
        public int Leave_Type_Id { get; set; }

        [ForeignKey(nameof(Leave_Type_Id))]
        public LeaveType LeaveType { get; set; }

        [Column("start_date", TypeName = "date")]
        [Required] 
        public DateTime Start_Date { get; set; }

        [Required] 
        public DateTime end_date { get; set; }

        [Required] 
        public int total_days { get; set; }

        [Required, StringLength(255)] 
        public string reason { get; set; }

        [StringLength(255)]
        public string? attachment { get; set; }

        [Required, StringLength(20)]
        public string status { get; set; } = "Pending";

        public int? approved_by_user_id { get; set; }

        [ForeignKey(nameof(approved_by_user_id))]
        public User Approved_by { get; set; }

        public DateTime? approved_at { get; set; }

        [StringLength(255)]
        public string? remark { get; set; }

        [Required] 
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }
    }
}
