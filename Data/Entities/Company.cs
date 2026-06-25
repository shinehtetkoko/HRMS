using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRMS.Interfaces;


namespace HRMS.Data.Entities
{
    [Table("tbl_company")]
    public class Company : IAuditable
    {
        [Key]
        [Column("comp_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Comp_Id { get; set; }

        [Required, StringLength(100)][Column("comp_name")] public string Comp_Name { get; set; }

        [Required, StringLength(20)][Column("comp_ph_no")] public string Comp_Ph_No { get; set; }

        [Required, StringLength(50)][Column("comp_email")] public string Comp_Email { get; set; }

        [Required, StringLength(255)][Column("comp_location")] public string Comp_Location { get; set; }

        [Required, StringLength(500)][Column("description")] public string Description { get; set; }

        [Required][Column("office_start_time")] public TimeSpan Office_Start_Time { get; set; }

        [Required][Column("office_end_time")] public TimeSpan Office_End_Time { get; set; }

        [Required][Column("created_at")] public DateTime Created_At { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }

        [Column("comp_dummy1")]
        [StringLength(255)]
        public string? Comp_Dummy1 { get; set; }

        [Column("comp_dummy2")]
        [StringLength(255)]
        public string? Comp_Dummy2 { get; set; }

    }
}