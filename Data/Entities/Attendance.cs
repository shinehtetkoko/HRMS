using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRMS.Interfaces;

namespace HRMS.Data.Entities {
    [Table("tbl_attendance")]
    public class Attendance : IAuditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("attendance_id")]
        public int Attendance_Id { get; set; }

        [Required]
        [Column("user_id")]
        public int User_Id { get; set; }

        [ForeignKey(nameof(User_Id))]
        public User User { get; set; }

        [Required]
        [Column("attendance_date", TypeName = "date")]
        public DateTime Attendance_Date { get; set; }

        [Required]
        [Column("check_in")]
        public DateTime Check_In { get; set; }

        [Column("check_out")]
        public DateTime? Check_Out { get; set; }

        [Required, StringLength(20)]
        [Column("attendance_status")]
        public string Attendance_Status { get; set; }

        [Column(TypeName = "numeric(4,2)")]

        public decimal? total_work_hours { get; set; }

        [Required, StringLength(20)]
        [Column("work_location")]
        public string Work_Location { get; set; }

        [Required, StringLength(20)]
        [Column("check_in_mode")]
        public string Check_In_Mode { get; set; }

        [StringLength(255)]
        [Column("attachment")]
        public string? Attachment { get; set; }

        [StringLength(255)]
        [Column("location_detail")]
        public string? Location_Detail { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }
    }
}
