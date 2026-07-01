using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    [Table("tbl_audit_log")]
    public class AuditLog
    {
        [Key]
        [Column("log_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Log_Id { get; set; }

        [Required]
        [Column("performed_account_id")]
        public int Performed_Account_Id { get; set; }
        [ForeignKey(nameof(Performed_Account_Id))]
        public UserAccount PerformedByAccount { get; set; }

        [Column("target_user_id")]
        public int? Target_User_Id { get; set; }

        [ForeignKey(nameof(Target_User_Id))]
        public User TargetUser { get; set; }

        [Required, StringLength(50)]
        [Column("module_name")]
        public string Module_Name { get; set; }

        [Required, StringLength(50)]
        [Column("action_type")]
        public string Action_Type { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}