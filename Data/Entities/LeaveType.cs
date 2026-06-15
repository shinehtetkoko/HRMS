using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class LeaveType
    {
        [Key]
        [Column("leave_type_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Leave_Type_Id { get; set; }

        [Required, StringLength(30)]
        [Column("leave_name")] 
        public string Leave_Name { get; set; }

        [Required]
        [Column("created_at")] 
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}