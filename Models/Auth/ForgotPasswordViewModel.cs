using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
