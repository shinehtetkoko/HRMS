using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Auth
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;
    }
}
