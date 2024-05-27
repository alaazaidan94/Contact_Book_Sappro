using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ContactBook_Services.AccountServices
{
    public class MailService : IMailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpClient> _logger;

        public MailService(
            IConfiguration configuration,
            ILogger<SmtpClient> logger
            )
        {
            _configuration = configuration;
            _logger = logger;
        }
        public async Task<bool> SendEmail(string toEmail,string subject, string body)
        {
            string FromEmail = _configuration["Email:From"];
            string Appkey = _configuration["Email:Appkey"];

            var message = new MailMessage()
            {
                From = new MailAddress("adasd@gmail.com"),
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            };

            foreach (var i in toEmail.Split(";"))
            {
                message.To.Add(new MailAddress(i));
            }

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(FromEmail, Appkey),
                EnableSsl = true,
            };

            try
            {
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", toEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", toEmail);
                return false;
            }

        }
    }
}
