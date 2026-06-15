using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class PublicHoliday
    {
        [Key]
        [Column("holiday_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Holiday_Id { get; set; }

        [Required, StringLength(100)]
        [Column("holiday_name")]
        public string Holiday_Name { get; set; }

        [Required, StringLength(20)]
        [Column("holiday_type")] 
        public string Holiday_Type { get; set; }

        [Required]
        [Column("start_date", TypeName = "date")]
        public DateTime Start_Date { get; set; }

        [Required]
        [Column("end_date", TypeName = "date")]
        public DateTime End_Date { get; set; }

        [Column("is_recurring")]
        public bool? Is_Recurring { get; set; }

        [Required]
        [Column("created_by_user_id")]
        public int Created_By_User_Id { get; set; }

        [ForeignKey(nameof(Created_By_User_Id))]
        public User Creator { get; set; }
        [Required]
        [Column("created_at")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}