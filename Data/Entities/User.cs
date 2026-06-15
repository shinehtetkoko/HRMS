using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class User
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int User_Id { get; set; }

        [Required][Column("dept_id")] 
        public int Dept_Id { get; set; }

        [ForeignKey(nameof(Dept_Id))]
        public Department Department { get; set; }

        [Required, StringLength(50)]
        [Column("user_name")] 
        public string User_Name { get; set; }

        [Required]
        [Column("gender")]
        public int Gender { get; set; }

        [Required, StringLength(50)]
        [Column("nrc")]
        public string Nrc { get; set; }

        [Required]
        [Column("dob", TypeName = "date")] 
        public DateTime Dob { get; set; }

        [Required]
        [Column("married_status")]
        public int Married_Status { get; set; }

        [Required, StringLength(50)]
        [Column("position")] 
        public string Position { get; set; }

        [Required]
        [Column("hired_date", TypeName = "date")]
        public DateTime Hired_Date { get; set; }

        [Required, StringLength(50)]
        [Column("qualification")] 
        public string Qualification { get; set; }

        [Required, StringLength(20)]
        [Column("user_ph_no")] 
        public string User_Ph_No { get; set; }

        [Required, StringLength(255)]
        [Column("address")] 
        public string Address { get; set; }

        [Required]
        [Column("is_active")]
        public bool Is_Active { get; set; } = true;

        [Required]
        [Column("created_at")] 
        public DateTime Created_At { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }
    }
}