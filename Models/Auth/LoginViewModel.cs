using System.ComponentModel.DataAnnotations;

namespace HRMS.Models.Auth
{
    public class LoginViewModel
    {
        public InputModel Input { get; set; } = new InputModel();
    }

    public class InputModel
    {
        [Required(ErrorMessage = "Email Address is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
