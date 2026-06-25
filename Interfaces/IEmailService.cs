using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IEmailService
    {
        // To send One-Time Password
        Task SendOneTimePasswordAsync(string toEmail, string userName, string tempPassword);

        Task SendResetPasswordEmailAsync(string email, string resetLink);
    }
}
