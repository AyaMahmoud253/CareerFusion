using Microsoft.Extensions.Options;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Web_API.Settings;
namespace Web_API.services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlContent, string plainTextContent)
        {
            var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.Username),
                Subject = subject,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(plainTextContent, null, "text/plain"));
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlContent, null, "text/html"));

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
