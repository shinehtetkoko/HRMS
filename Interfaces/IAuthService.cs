using HRMS.Models.Auth;
using HRMS.Data.Entities;
using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IAuthService
    {
        // IsFirstLogin & Email
        Task<(bool Success, string Message, bool IsFirstLogin, string Email, string RoleName, string User_Name)> ValidateLoginAsync(string email, string password);

        // Forgot Password 
        Task<(bool Success, string Message, string Token)> VerifyForgotPasswordAsync(string email);

        // Change Password
        Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordViewModel model);

        Task<bool> VerifyResetTokenAsync(string email, string token);

        Task<(bool Success, string Message)> ResetPasswordAsync(ChangePasswordViewModel model);
    }
}