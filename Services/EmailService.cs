using HRMS.Interfaces;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace HRMS.Services
{
    public class EmailService : IEmailService
    {
        // Company mail
        private readonly string _fromEmail = "yinwailwin820@gmail.com"; // Company mail
        private readonly string _fromPassword = "hqzf jdbq xvfi cnhe"; // App password 

        public async Task SendOneTimePasswordAsync(string toEmail, string userName, string tempPassword)
        {
            try
            {
                // Config Gmail SMTP Server 
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587, // Port No to Gmail
                    Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                    EnableSsl = true, // For security
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "HRMS Admin System"),
                    Subject = "Your HRMS Temporary Account One-Time Password",

                    Body = $@"
                        <div style='font-family: Inter, sans-serif; max-width: 600px; margin: auto; padding: 20px; border: 1px solid #e2e8f0; rounded-direction: 8px;'>
                            <h2 style='color: #1e3a8a; border-bottom: 2px solid #3b82f6; padding-bottom: 10px;'>HRMS Account Activation</h2>
                            <p style='font-size: 14px; color: #334155;'>Dear <strong>{userName}</strong>,</p>
                            <p style='font-size: 14px; color: #334155;'>Your official account has been successfully registered inside our HRMS System.</p>
                            <p style='font-size: 14px; color: #334155;'>Please use the temporary <strong>One-Time Password (OTP)</strong> below to log in for the first time:</p>
                            
                            <div style='text-align: center; margin: 25px 0;'>
                                <span style='font-size: 24px; font-weight: bold; color: #1d4ed8; background-color: #eff6ff; border: 1px dashed #3b82f6; padding: 12px 30px; letter-spacing: 2px; display: inline-block; border-radius: 6px;'>
                                    {tempPassword}
                                </span>
                            </div>

                            <p style='font-size: 13px; color: #ef4444; font-weight: bold;'>⚠️ Security Notice:</p>
                            <p style='font-size: 13px; color: #64748b; margin-top: -10px;'>You will be automatically prompted to change this password immediately upon your first login for security reasons.</p>
                            <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                            <p style='font-size: 11px; color: #94a3b8; text-align: center;'>This is an automated system email. Please do not reply directly to this message.</p>
                        </div>",
                    IsBodyHtml = true
                };

                // Registered Real Email 
                mailMessage.To.Add(toEmail);

                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Email Send Error: {ex.Message}");
                throw new Exception("Failed to send automated email. Technical info: " + ex.Message);
            }
        }

        // Send Reset Password link

        public async Task SendResetPasswordEmailAsync(string email, string resetLink)
        {
            try
            {
                using var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(_fromEmail, _fromPassword),
                    EnableSsl = true,
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_fromEmail, "HRMS Security System"),
                    Subject = "🔄 HRMS Account Password Reset Request",
                    Body = $@"
                        <div style='font-family: Inter, sans-serif; max-width: 600px; margin: auto; padding: 25px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                            <h2 style='color: #1e3a8a; border-bottom: 2px solid #3b82f6; padding-bottom: 10px;'>Password Reset Request</h2>
                            <p style='font-size: 14px; color: #334155;'>Hello,</p>
                            <p style='font-size: 14px; color: #334155;'>We received a request to reset the password for your HRMS account. Click the button below to choose a new password:</p>
                            
                            <div style='text-align: center; margin: 30px 0;'>
                                <a href='{resetLink}' style='background-color: #2563eb; color: white; padding: 12px 30px; text-decoration: none; border-radius: 6px; font-weight: bold; font-size: 15px; display: inline-block; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1);'>
                                    Reset Password
                                </a>
                            </div>

                            <p style='font-size: 13px; color: #64748b;'>This link is secure and will expire in <strong>2 hours</strong> for security reasons.</p>
                            <p style='font-size: 13px; color: #94a3b8; margin-top: -5px;'>If you did not request a password reset, please disregard this email safely.</p>
                            <hr style='border: 0; border-top: 1px solid #e2e8f0; margin: 20px 0;'>
                            <p style='font-size: 11px; color: #94a3b8; text-align: center;'>Lumi Tech HRMS System. All rights reserved.</p>
                        </div>",
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SMTP Reset Email Send Error: {ex.Message}");
                throw new Exception("Failed to send reset password email. Technical info: " + ex.Message);
            }
        }


    }
}