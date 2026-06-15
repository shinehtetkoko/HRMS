using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class LeavePolicy
    {
        [Key]
        [Column("policy_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Policy_Id { get; set; }

        [Required]
        [Column("leave_type_id")]
        public int Leave_Type_Id { get; set; }

        [ForeignKey(nameof(Leave_Type_Id))]
        public LeaveType LeaveType { get; set; }

        [Required]
        [Column("total_days")]
        public int Total_Days { get; set; }

        [Required]
        [Column("carry_forward")]
        public bool Carry_Forward { get; set; }

        [Column("carry_duration")]
        public int? Carry_Duration { get; set; }

        [Column("max_carry_day")]
        public int? Max_Carry_Day { get; set; }

        [Required]
        [Column("reset_cycle", TypeName = "date")]
        public DateTime Reset_Cycle { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}