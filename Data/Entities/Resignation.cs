using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities {
    public class Resignation
    {
        [Key]
        [Column("resignation_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Resignation_Id { get; set; }

        [Required]
        [Column("user_id")] 
        public int User_Id { get; set; }

        [ForeignKey(nameof(User_Id))]
        public User User { get; set; }

        [Required]
        [Column("resignation_date", TypeName = "date")] 
        public DateTime Resignation_Date { get; set; }

        [Required, StringLength(255)]
        [Column("resignation_reason")] 
        public string Resignation_Reason { get; set; }

        [Required]
        [Column("resigned_by_user_id")]
        public int Resigned_By_User_Id { get; set; }

        [ForeignKey(nameof(Resigned_By_User_Id))]
        public User HRApprover { get; set; }

        [Required]
        [Column("created_at")] 
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
