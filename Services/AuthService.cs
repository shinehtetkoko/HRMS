using HRMS.Data;
using HRMS.Data.Entities;
using HRMS.Interfaces;
using HRMS.Models.Auth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace HRMS.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        // ValidateLoginAsync
        public async Task<(bool Success, string Message, bool IsFirstLogin, string Email, string RoleName, string User_Name)> ValidateLoginAsync(string email, string password)
        {
            var account = await _context.Set<UserAccount>()
                .Include(ua => ua.Role)
                .Include(ua => ua.User)
                .FirstOrDefaultAsync(u => u.Email == email);

            if (account == null)
            {
                return (false, "Invalid email or password.", false, "", "", "");
            }

            string dbRole = account.Role != null ? account.Role.Role_Name : "Employee";

            if (!dbRole.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (account.User == null || !account.User.Is_Active)
                {
                    return (false, "This account has been deactivated.", false, "", "", "");
                }
            }

            string displayName = account.User != null ? account.User.User_Name : "Admin";

            // Check password
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.Password_Hash);
            if (!isPasswordValid)
            {
                return (false, "Invalid email or password.", false, "", "", "");
            }

            return (true, "Login successful!", account.Is_First_Login, account.Email, dbRole, displayName);
        }


        // ChangePasswordAsync - First Time Login
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

        // VerifyForgotPasswordAsync
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

            // Cryptographically Secure 64-character Hex Token 
            string secureToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));

            // Temporary store(2hour)
            account.Password_Reset_Token = secureToken;
            account.Token_Expiry = DateTime.UtcNow.AddHours(2);

            _context.Set<UserAccount>().Update(account);
            await _context.SaveChangesAsync();

            return (true, "Verification complete!", secureToken);
        }

        // VerifyResetTokenAsync 
        public async Task<bool> VerifyResetTokenAsync(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token)) return false;

            var account = await _context.Set<UserAccount>()
                .FirstOrDefaultAsync(u => u.Email == email && u.Password_Reset_Token == token);

            // If expire
            if (account == null || account.Token_Expiry < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        //  ResetPasswordAsync 
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

    }
}