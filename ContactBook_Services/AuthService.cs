using AutoMapper;
using ContactBook_Domain.Models;
using ContactBook_Services.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ContactBook_Services
{
    public class AuthService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly MailService _mailService;
        private readonly CompanyService _companyService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            MailService mailService,
            CompanyService companyService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mailService = mailService;
            _companyService = companyService;
            _configuration = configuration;
            _mapper = mapper;
        }

        public async Task<bool> CheckAccount(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);

            if (user == null)
                return false;

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);

            if (!result.Succeeded)
                return false;

            return true;
        }
        public async Task<bool> ChechConfirmEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user!.EmailConfirmed == false)
                return false;

            return true;
        }
        public async Task<bool> CreateUserAndCompany(RegisterDTO registerDTO)
        {
            Company company = _mapper.Map<Company>(registerDTO);

            if (!await _companyService.AddAsync(company))
                return false;

            User user = _mapper.Map<RegisterDTO, User>(registerDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.CompanyId = company.CompanyId;
                    dest.UserName = registerDTO.Email;
                    dest.Role = Roles.Admin;
                });
            });

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (!result.Succeeded)
            {
                await _companyService.DeleteAsync(company.CompanyId);
                return false;
            }

            return true;
        }
        public async Task<UserDTO> CreateUserDTO(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return null;

            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var days = int.Parse(_configuration["JWT:AddDays"]!);

            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(days),
                SigningCredentials = credentials,
                Issuer = _configuration["JWT:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            var userDTO = new UserDTO
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                Exp = jwt.ValidTo,
                Token = tokenHandler.WriteToken(jwt)
            };

            return await Task.FromResult(userDTO);
        }
        public async Task<bool> ConfirmEmail(string email, string token)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return false;

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (result.Succeeded)
            {
                user.Status = UserStatus.Active;
                await _userManager.UpdateAsync(user);
                return true;
            }
            return false;
        }
        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return false;

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            if (!result.Succeeded)
                return false;

            return true;
        }
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
        public async Task<bool> SendConfirmEMailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var url = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?email={user.Email}&token={token}";

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
        public async Task<bool> SendConfirmAndSetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

            var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenSetPass = await _userManager.GeneratePasswordResetTokenAsync(user);

            tokenConfirm = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirm));
            tokenSetPass = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenSetPass));

            var urlConfirm = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?email={user.Email}&token={tokenConfirm}";
            var urlSetPass = $"{_configuration["appUrl"]}/{_configuration["Email:SetPasswordPath"]}?email={user.Email}&token={tokenSetPass}";

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
        public async Task<bool> SendForgotPasswordEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

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
