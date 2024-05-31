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

        public async Task<(bool, string)> CheckAccount(LoginDTO loginDTO)
        {
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null || user.isDeleted == true)
                return (false, "Your account was not found.");

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDTO.Password, false);
            if (!result.Succeeded)
                return (false, "Invalid username or password");

            if (user.Status == UserStatus.Locked)
                return (false, "Your account hass been locked, please contact admin");

            return (true,"Login success");
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
            // Map RegisterDTO to Company
            Company company = _mapper.Map<Company>(registerDTO);

            // Add company to database
            if (!await _companyService.AddAsync(company))
                return false;

            // Map RegisterDTO to User and set additional properties
            User user = _mapper.Map<RegisterDTO, User>(registerDTO, opts =>
            {
                opts.AfterMap((src, dest) =>
                {
                    dest.CompanyId = company.CompanyId;
                    dest.UserName = registerDTO.Email;
                    dest.Role = Roles.Owner;
                });
            });

            // Create user with specified role and password
            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            // If user creation fails, delete the created company and return false
            if (!result.Succeeded)
            {
                await _companyService.DeleteAsync(company.CompanyId);
                return false;
            }

            // Return true if user and company creation are successful
            return true;
        }
        public async Task<UserDTO> CreateUserDTO(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return null;

            // Retrieve JWT configuration settings
            var jwtKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
            var days = int.Parse(_configuration["JWT:AddDays"]!);

            // Define user claims for JWT token
            var userClaims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
            };

            // Create JWT token credentials
            var credentials = new SigningCredentials(jwtKey, SecurityAlgorithms.HmacSha512Signature);

            // Define token descriptor
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(userClaims),
                Expires = DateTime.UtcNow.AddDays(days),
                SigningCredentials = credentials,
                Issuer = _configuration["JWT:Issuer"]
            };

            // Create JWT token handler
            var tokenHandler = new JwtSecurityTokenHandler();

            // Generate JWT token
            var jwt = tokenHandler.CreateToken(tokenDescriptor);

            // Create UserDTO object
            var userDTO = new UserDTO
            {
                UserId = user.Id,
                FullName = $"{user.FirstName} {user.LastName}",
                Role = user.Role,
                Exp = jwt.ValidTo,
                Token = tokenHandler.WriteToken(jwt)
            };

            // Return the UserDTO object
            return await Task.FromResult(userDTO);
        }
        public async Task<bool> ConfirmEmail(string email, string token)
        {
            // Find the user by email
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return false;

            // Decode the token
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            // Confirm email using the token
            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            // If email confirmation succeeds
            if (result.Succeeded)
            {
                // Set user status to Active and update user
                user.Status = UserStatus.Active;
                await _userManager.UpdateAsync(user);
                return true;
            }

            // If email confirmation fails, return false
            return false;
        }
        public async Task<bool> ResetPassword(string email, string token, string newPassword)
        {
            // Find the user by email
            var user = await _userManager.FindByNameAsync(email);

            if (user == null)
                return false;

            // Decode the token
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            // Reset user password using the token
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, newPassword);

            // If password reset succeeds, return true; otherwise, return false
            return result.Succeeded;
        }
        public async Task<bool> CheckEmailExistsAsync(string email)
        {
            return await _userManager.Users.AnyAsync(x => x.Email == email.ToLower());
        }
        public async Task<bool> SendConfirmEMailAsync(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

            // Generate email confirmation token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Encode the token
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Construct confirmation URL
            var url = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?email={user.Email}&token={token}";

            // Construct email body
            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>Please confirm your email address by clicking on the following link.</p>" +
                $"<p><a href=\"{url}\">Click here</a></p>" +
                "<p>Thank you,</p>" +
                $"<br>{_configuration["Email:ApplicationName"]}";

            // Send confirmation email
            var result = await _mailService.SendEmail(user.Email!, "ConfirmEmail", body);

            // Return true if email was sent successfully; otherwise, false
            return result;
        }
        public async Task<bool> SendConfirmAndSetPasswordAsync(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            // If user not found, return false
            if (user == null)
                return false;

            // Generate email confirmation and password reset tokens
            var tokenConfirm = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenSetPass = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the tokens
            tokenConfirm = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenConfirm));
            tokenSetPass = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(tokenSetPass));

            // Construct confirmation and set password URLs
            var urlConfirm = $"{_configuration["appUrl"]}/{_configuration["Email:ConfirmEmailPath"]}?email={user.Email}&token={tokenConfirm}";
            var urlSetPass = $"{_configuration["appUrl"]}/{_configuration["Email:SetPasswordPath"]}?email={user.Email}&token={tokenSetPass}";

            // Construct email body
            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
                "<p>You have been invited to try the SAPPRO application.</p>" +
                "<p>Click here to confirm your email.</p>" +
                $"<p><a href=\"{urlConfirm}\">Click here</a></p>" +
                "<p>Click here to create a password.</p>" +
                $"<p><a href=\"{urlSetPass}\">Click here</a></p>" +
                "<p>Thank you,</p>" +
                $"<br>{_configuration["Email:ApplicationName"]}";

            // Send invitation email
            var result = await _mailService.SendEmail(user.Email!, "Invite", body);

            // Return true if email was sent successfully; otherwise, false
            return result;
        }
        public async Task<bool> SendForgotPasswordEmail(string email)
        {
            // Find the user by email
            var user = await _userManager.FindByEmailAsync(email);

            // If user not found, return false
            if (user == null)
                return false;

            // Generate password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Encode the token
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Construct reset password URL
            var url = $"{_configuration["appUrl"]}/{_configuration["Email:ResetPasswordPath"]}?email={user.Email}&token={token}";

            // Construct email body
            var body = $"<p>Hello: {user.FirstName} {user.LastName}</p>" +
               $"<p>Username: {user.UserName}.</p>" +
               "<p>In order to reset your password, please click on the following link.</p>" +
               $"<p><a href=\"{url}\">Click here</a></p>" +
               "<p>Thank you,</p>" +
               $"<br>{_configuration["Email:ApplicationName"]}";

            // Send forgot password email
            var result = await _mailService.SendEmail(user.Email!, "Forgot password", body);

            // Return true if email was sent successfully; otherwise, false
            return result;
        }
    }
}
