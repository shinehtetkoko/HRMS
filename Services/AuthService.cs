using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Enums;
using HRMS.Interfaces;
using HRMS.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace HRMS.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        #region User Authentication (Login)
        /// <summary>
        /// Validates user login credentials and checks if the account is active or if it is their first time logging in.
        /// </summary>
        /// <param name="email">The user email address.</param>
        /// <param name="password">The plain text password entered by the user.</param>
        /// <returns>A tuple with success status, display messages, first-login flag, and user metadata details.</returns>
        public async Task<(bool Success, string Message, bool IsFirstLogin, int AccountId, string Email, string RoleName, string User_Name)> ValidateLoginAsync(string email, string password)
        {
            var account = await _context.Set<UserAccount>()
                .Include(u => u.Role).Include(u => u.User)
                .FirstOrDefaultAsync(u => u.Email == email);
            if (account == null)
            {
                return (false, "Invalid email or password.", false, "", "", "", 0);
            }

            string dbRole = account.Role?.Role_Name ?? "Employee";

            if (!dbRole.Equals(UserRole.Admin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (account.User == null || !account.User.Is_Active)
                {
                    return (false, "This account has been deactivated.", false, "", "", "", 0);
                }
            }

            string displayName = account.User?.User_Name ?? "Admin";

            bool isPasswordValid;

            try
            {
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.Password_Hash);
            }
            catch
            {
                return (false, "Invalid email or password.", false, 0, "", "", "");
            }

            if (!isPasswordValid)
            {
                return (false, "Invalid email or password.", false, "", "", "", 0);
            }

            return (true, "Login successful!", account.Is_First_Login, account.Account_Id, account.Email, dbRole, displayName);
        }
        #endregion

        #region Password Updates (First Login)
        /// <summary>
        /// Updates the password hash for a user during their first-time login sequence.
        /// </summary>
        /// <param name="model">The data payload containing user email and the newly selected password.</param>
        /// <returns>A tuple indicating success status and result messages.</returns>
        public async Task<(bool Success, string Message)> ChangePasswordAsync(ChangePasswordViewModel model)
        {
            var account = await _context.Set<UserAccount>()
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (account == null)
            {
                return (false, "User account not found.");
            }

            account.Password_Hash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            account.Is_First_Login = false;

            _context.Set<UserAccount>().Update(account);
            await _context.SaveChangesAsync();

            return (true, "Password updated successfully!");
        }
        #endregion

        #region Account Recovery & Password Reset
        /// <summary>
        /// Generates a secure token and sets a 2-hour expiration lifespan for forgot password workflows.
        /// </summary>
        /// <param name="email">The email account requesting the password recovery link.</param>
        /// <returns>A tuple indicating verification status and the newly generated hex token string.</returns>
        public async Task<(bool Success, string Message, string Token)> VerifyForgotPasswordAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return (false, "Email is required.", "");
            }

            if (email.Equals("admin@hrsystem.com", StringComparison.OrdinalIgnoreCase))
            {
                return (false, "The Administrator account password cannot be reset via the public form.", "");
            }

            var account = await _context.Set<UserAccount>().FirstOrDefaultAsync(e => e.Email == email);
            if (account == null)
            {
                return (false, "This email address is not registered in our system.", "");
            }

            string secureToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            account.Password_Reset_Token = secureToken;
            account.Token_Expiry = DateTime.UtcNow.AddHours(2);

            _context.Set<UserAccount>().Update(account);
            await _context.SaveChangesAsync();

            return (true, "Verification complete!", secureToken);
        }

        /// <summary>
        /// Checks if a given password reset token matches the user account and is still within its valid lifespan.
        /// </summary>
        /// <param name="email">The email address tied to the recovery request session.</param>
        /// <param name="token">The verification token hash string to validate.</param>
        /// <returns>True if the token matches and is still active; otherwise, returns false.</returns> 
        public async Task<bool> VerifyResetTokenAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return false;

            var account = await _context.Set<UserAccount>()
                .FirstOrDefaultAsync(u => u.Email == email && u.Password_Reset_Token == token);

            if (account == null || account.Token_Expiry < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Applies the new password to the user account using verified recovery token parameters.
        /// </summary>
        /// <param name="model">The payload containing the recovery token, user email, and new password entries.</param>
        /// <returns>A tuple indicating success status and result messages.</returns>
        public async Task<(bool Success, string Message)> ResetPasswordAsync(ChangePasswordViewModel model)
        {
            var account = await _context.Set<UserAccount>()
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password_Reset_Token == model.Token);

            if (account == null || account.Token_Expiry < DateTime.UtcNow)
            {
                return (false, "Invalid or expired password reset session.");
            }

            account.Password_Hash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            account.Is_First_Login = false;

            account.Password_Reset_Token = null;
            account.Token_Expiry = null;

            _context.Set<UserAccount>().Update(account);
            await _context.SaveChangesAsync();

            return (true, "Your password has been reset successfully!");
        }
        #endregion
    }
}