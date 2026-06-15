using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class LeaveBalance
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("balance_id")]
        public int Balance_Id { get; set; }

        [Required]
        [Column("leave_type_id")]
        public int Leave_Type_Id { get; set; }

        [ForeignKey(nameof(Leave_Type_Id))]
        public LeaveType LeaveType { get; set; }

        [Required]
        [Column("user_id")]
        public int User_Id { get; set; }

        [ForeignKey(nameof(User_Id))]
        public User User { get; set; }

        [Required]
        [Column("year")]
        public int Year { get; set; }

        [Required]
        [Column("allocated_days")]
        public int Allocated_Days { get; set; }

        [Required]
        [Column("used_days")]
        public int Used_Days { get; set; }

        [Required]
        [Column("remaining_days")]
        public int Remaining_Days { get; set; }

        [Column("carried_forward_days")]
        public int? Carried_Forward_Days { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime? updated_at { get; set; }
    }
}