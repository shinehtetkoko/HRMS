using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities {
    [Table("tbl_role")]
    public class Role
    {
        [Key]
        [Column("role_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Role_Id { get; set; }

        [Required, StringLength(20)]
        [Column("role_name")]
        public string Role_Name { get; set; }
    }
}
