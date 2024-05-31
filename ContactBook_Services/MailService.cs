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
            // Create a new email message
            var message = new MailMessage()
            {
                From = new MailAddress("test@gmail.com"),
                Subject = subject,
                IsBodyHtml = true,
                Body = body,
            };

            // Add recipients
            foreach (var i in toEmail.Split(";"))
            {
                message.To.Add(new MailAddress(i));
            }

            // Send the email using Google SMTP
            if (!await GoogleSMTP(message))
                return false;

            return true;

        }
        public async Task<bool> SendContactEmail(SendEmailDTO sendEmailDTO)
        {
            // Create a new email message
            var message = new MailMessage
            {
                From = new MailAddress("test@gmail.com"),
                Subject = sendEmailDTO.Subject,
                IsBodyHtml = true,
                Body = sendEmailDTO.Body
            };

            // Add recipients
            foreach (var to in sendEmailDTO.To.Split(";"))
            {
                message.To.Add(new MailAddress(to));
            }

            // Add CC recipients if provided
            if (sendEmailDTO.CC != null)
            {
                foreach (var cc in sendEmailDTO.CC.Split(";"))
                {
                    message.CC.Add(new MailAddress(cc));
                }
            }

            // Add BCC recipients if provided
            if (sendEmailDTO.BCC != null)
            {
                foreach (var bcc in sendEmailDTO.BCC.Split(";"))
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }
            }

            // Send the email using Google SMTP
            if (!await GoogleSMTP(message))
                return false;

            // Return the result of the email send operation
            return true;
        }
        public async Task<bool> SendExportContactEmail(string toEmail, string exportPath)
        {
            // Create a new email message
            var message = new MailMessage()
            {
                From = new MailAddress("test@gmail.com"),
                Subject = "Contacts_Reports",
                IsBodyHtml = true,
                Body = "",
            };

            // Add recipients
            foreach (var i in toEmail.Split(";"))
            {
                message.To.Add(new MailAddress(i));
            }

            if (!string.IsNullOrEmpty(exportPath))
            {
                var attachment = new Attachment(exportPath);
                message.Attachments.Add(attachment);
            }

            // Send the email using Google SMTP
            if (!await GoogleSMTP(message))
                return false;

            return true;

        }
        private async Task<bool> GoogleSMTP(MailMessage message)
        {
            // Retrieve email configuration settings
            string emailMain = _configuration["Email:EmailMain"]!;
            string appKey = _configuration["Email:Appkey"]!;

            // Configure the SMTP client
            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(emailMain, appKey),
                EnableSsl = true,
            };

            try
            {
                // Send the email asynchronously
                await smtp.SendMailAsync(message);

                _logger.LogInformation("Email sent successfully to {ToEmail}", message.To);
               
                return true;
            }
            catch (Exception ex)
            {
                // Log any errors that occur during sending
                _logger.LogError(ex, "Error sending email to {ToEmail}", message.To);
               
                return false;
            }
        }

    }
}
