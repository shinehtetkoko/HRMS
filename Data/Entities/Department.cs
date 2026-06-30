using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HRMS.Interfaces;

namespace HRMS.Data.Entities
{
    [Table("tbl_department")]
    public class Department : IAuditable
    {
        [Key]
        [Column("dept_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]

        public int DepartmentId { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("dept_name")]
        public string DepartmentName { get; set; } = string.Empty;

        [Column("dept_head_user_id")]
        public int? DeptHeadUserId { get; set; }

        [ForeignKey(nameof(DeptHeadUserId))]
        public virtual User? DeptHeadUser { get; set; }

        public virtual ICollection<User> Users { get; set; } = new List<User>();

        //[Required]
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? updated_at { get; set; }
    }
}