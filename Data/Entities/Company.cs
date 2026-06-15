using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class Company
    {
        [Key]
        [Column("comp_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Comp_Id { get; set; }

        [Required, StringLength(100)]
        [Column("comp_name")] 
        public string Comp_Name { get; set; }

        [Required, StringLength(20)]
        [Column("comp_ph_no")] 
        public string Comp_Ph_No { get; set; }

        [Required, StringLength(50)]
        [Column("comp_email")] 
        public string Comp_Email { get; set; }

        [Required, StringLength(255)]
        [Column("comp_location")] 
        public string Comp_Location { get; set; }

        [Required, StringLength(500)]
        [Column("description")]
        public string Description { get; set; }

        [Required]
        [Column("office_start_time")] 
        public TimeSpan Office_Start_Time { get; set; }

        [Required]
        [Column("office_end_time")] 
        public TimeSpan Office_End_Time { get; set; }

        [Required]
        [Column("created_at")]
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTime Updated_At { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }
    }
}