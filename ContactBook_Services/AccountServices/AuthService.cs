using ContactBook_Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Text;

namespace ContactBook_Services.AccountServices
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<User> userManager,
            IMailService mailService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _mailService = mailService;
            _configuration = configuration;
        }
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
        public async Task<bool> SendConfirmEMailAsync(User user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?userId={user.Id}&token={token}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you,</p>" +
                $"<br>{_configuration["Email:ApplicationName"]}";

            var result = await _mailService.SendEmail(user.Email!, "ConfirmEmail", body);

            if (result)
            {
                return true;
            }

            return false;
        }
        public async Task<bool> SendConfirmAndSetPasswordAsync(User user)
        {
            var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenSetPass = await _userManager.GeneratePasswordResetTokenAsync(user);

            tokenConfirm = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirm));
            tokenSetPass = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenSetPass));

            var urlConfirm = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?userId={user.Id}&token={tokenConfirm}";
            var urlSetPass = $"{_configuration["appUrl"]}/{_configuration["Email:SetPasswordPath"]}?userId={user.Id}&token={tokenSetPass}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>You have been invited to try the SAPPRO application.</p>" +
                "<p>Click here to confirm your email.</p>" +
                $"<p><a href=\"{urlConfirm}\">Click here</a></p>" +
                "<p>Click here to create a password.</p>" +
                $"<p><a href=\"{urlSetPass}\">Click here</a></p>" +
                "<p>Thank you,</p>" +
                $"<br>{_configuration["Email:ApplicationName"]}";

            var result = await _mailService.SendEmail(user.Email!, "Invite", body);

            if (result)
            {
                return true;
            }

            return false;
        }
        public async Task<bool> SendForgotPasswordEmail(User user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{_configuration["appUrl"]}/{_configuration["Email:ResetPasswordPath"]}?email={user.Email}&token={token}";

            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
               $"<p>Username: {user.UserName}.</p>" +
               "<p>In order to reset your password, please click on the following link.</p>" +
               $"<p><a href=\"{url}\">Click here</a></p>" +
               "<p>Thank you,</p>" +
               $"<br>{_configuration["Email:ApplicationName"]}";

            var result = await _mailService.SendEmail(user.Email!, "Forgot password", body);

            if (result)
            {
                return true;
            }

            return false;
        }
    }
}
