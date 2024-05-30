using ContactBook_Services.DTOs.Contact;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace ContactBook_Services
{
    public class MailService
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
        public async Task<bool> SendEmail(string toEmail, string subject, string body)
        {
            string FromEmail = _configuration["Email:From"];
            string Appkey = _configuration["Email:Appkey"];

            var message = new MailMessage()
            {
                From = new MailAddress("tawasul.syria@gmail.com"),
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            };

            foreach (var i in toEmail.Split(";"))
            {
                message.To.Add(new MailAddress(i));
            }

            if (!await GoogleSMTP(message))
                return false;

            return true;

        }
        public async Task<bool> SendContactEmail(SendEmailDTO sendEmailDTO)
        {

            var message = new MailMessage()
            {
                From = new MailAddress("tawasul.syria@gmail.com"),
                Subject = sendEmailDTO.Subject,
                IsBodyHtml = true,
                Body = sendEmailDTO.Body,
            };

            foreach (var to in sendEmailDTO.To.Split(";"))
            {
                message.To.Add(new MailAddress(to));
            }

            if (sendEmailDTO.CC != null)
            {
                foreach (var cc in sendEmailDTO.CC.Split(";"))
                {
                    message.CC.Add(new MailAddress(cc));
                }
            }

            if (sendEmailDTO.BCC != null)
            {
                foreach (var bcc in sendEmailDTO.BCC.Split(";"))
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }
            }

            if (!await GoogleSMTP(message))
                return false;

            
            return true;
        }
        private async Task<bool> GoogleSMTP(MailMessage message)
        {

            string EmailMain = _configuration["Email:EmailMain"];
            string Appkey = _configuration["Email:Appkey"];

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(EmailMain, Appkey),
                EnableSsl = true,
            };

            try
            {
                await smtp.SendMailAsync(message);
                _logger.LogInformation("Email sent successfully to {ToEmail}", message.To);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email to {ToEmail}", message.To);
                return false;
            }
        }
    }
}
