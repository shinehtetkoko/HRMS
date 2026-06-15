using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    public class UserAccount
    {
        [Key]
        [Column("account_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Account_Id { get; set; }

        [Column("user_id")]
        public int? User_Id { get; set; }

        [ForeignKey(nameof(User_Id))]
        public User User { get; set; }

        [Required]
        [Column("role_id")] 
        public int Role_Id { get; set; }

        [ForeignKey(nameof(Role_Id))]
        public Role Role { get; set; }

        [Required, StringLength(50)]
        [Column("email")] 
        public string Email { get; set; }

        [Required, StringLength(255)]
        [Column("password_hash")] 
        public string Password_Hash { get; set; }

        [Required]
        [Column("is_first_login")] 
        public bool Is_First_Login { get; set; }

        [Column("last_login")]
        public DateTime? Last_Login { get; set; }

        [Required]
        [Column("created_at")] 
        public DateTime Created_At { get; set; } = DateTime.UtcNow;
    }
}
