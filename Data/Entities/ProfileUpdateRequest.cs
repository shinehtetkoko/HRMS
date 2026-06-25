using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HRMS.Data.Entities
{
    [Table("tbl_profile_update_request")]
    public class ProfileUpdateRequest
    {
        [Key]
        [Column("request_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }

        [Required]
        [Column("user_id")]
        public int UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        [Column("new_ph_no")]
        public string? NewPhoneNumber { get; set; }

        [Column("new_address")]
        public string? NewAddress { get; set; }

        [Required]
        [Column("status")]
        public string Status { get; set; } = "Pending";

        [Column("reviewed_by_user_id")]
        public int? ReviewedByUserId { get; set; }

        [ForeignKey(nameof(ReviewedByUserId))]
        public User Reviewer { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        [Required]
        [Column("created_at")]

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}