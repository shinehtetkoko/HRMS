using System.Threading.Tasks;

namespace HRMS.Interfaces
{
    public interface IEmailService
    {
        Task SendOneTimePasswordAsync(string toEmail, string userName, string tempPassword);

        Task SendResetPasswordEmailAsync(string email, string resetLink);
    }
}
