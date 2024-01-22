using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Web_API.helpers;
using Web_API.Models;

namespace Web_API.services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private IConfiguration _configuration;
        private IMailService _mailService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public AuthService(
            UserManager<ApplicationUser> userManager, IConfiguration configuration, IMailService mailService,
            RoleManager<IdentityRole> roleManager,
            IOptions<JWT> jwt)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _configuration = configuration;
            _mailService = mailService;
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            _jwt = jwt?.Value ?? throw new ArgumentNullException(nameof(jwt));

            if (_jwt == null)
            {
                // Log or throw an exception to capture this scenario
                throw new InvalidOperationException("JWT configuration is null");
            }
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message = "Email is already registered!" };

            if (await _userManager.FindByNameAsync(model.UserName) is not null)
                return new AuthModel { Message = "Username is already registered!" };

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(",", result.Errors.Select(error => error.Description));
                return new AuthModel { Message = errors };
            }
            var confirmEmailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);

            string url = $"{_configuration["AppUrl"]}/api/Auth/confirmemail?userid={user.Id}&token={validEmailToken}";

            await _mailService.SendEmailAsync(user.Email, "Confirm your email", $"<h1>Welcome to Career Fusion App</h1>" +
                    $"<p>Please confirm your email by <a href='{url}'>Clicking here</a></p>");
            await _userManager.AddToRoleAsync(user, "User");
            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                UserName = user.UserName
            };
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user is null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Message = "Email or Password is incorrect!";
                return authModel;
            }

            var jwtSecurityToken = await CreateJwtToken(user);
            var rolesList = await _userManager.GetRolesAsync(user);

            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            authModel.Email = user.Email;
            authModel.UserName = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles = rolesList.ToList();

            return authModel;
        }

        public async Task<AuthModel> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = "User not found"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ConfirmEmailAsync(user, normalToken);
            if (result.Succeeded)
                return new AuthModel
                {
                    Message = "Email confirmed successfully!",
                    IsAuthenticated = true,
                };
            return new AuthModel
            {
                IsAuthenticated = false,
                Message = "Email did not confirm. " + string.Join(", ", result.Errors.Select(e => e.Description))
            };




        }

        public async Task<AuthModel> ForgetPasswordAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = "No user associated with email",
                };

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = Encoding.UTF8.GetBytes(token);
            var validToken = WebEncoders.Base64UrlEncode(encodedToken);

            string url = $"{_configuration["AppUrl"]}/ResetPassword?email={email}&token={validToken}";

            await _mailService.SendEmailAsync(email, "Reset Password", "<h1>Follow the instructions to reset your password</h1>" +
                $"<p>To reset your password <a href='{url}'>Click here</a></p>");

            return new AuthModel
            {
                IsAuthenticated = true,
                Message = "Reset password URL has been sent to the email successfully!"
            };

        }

        public async Task<AuthModel> ResetPasswordAsync(ResetPasswordViewModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = "No user associated with email",
                };

            if (model.NewPassword != model.ConfirmPassword)
                return new AuthModel
                {
                    IsAuthenticated = false,
                    Message = "Password doesn't match its confirmation",
                };

            var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManager.ResetPasswordAsync(user, normalToken, model.NewPassword);

            if (result.Succeeded)
                return new AuthModel
                {
                    Message = "Password has been reset successfully!",
                    IsAuthenticated = true,
                };

            return new AuthModel
            {
                IsAuthenticated = false,
                Message = "Something went wrong. " + string.Join(", ", result.Errors.Select(e => e.Description))
            };






        }


        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid user ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Something went wrong";
        }
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(role => new Claim("roles", role)).ToList();

            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? throw new ArgumentNullException(nameof(user.UserName))),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? throw new ArgumentNullException(nameof(user.Email))),
        new Claim("uid", user.Id ?? throw new ArgumentNullException(nameof(user.Id)))
    }
            .Union(userClaims)
            .Union(roleClaims);

            // Hardcoded JWT configuration
            var jwtIssuer = "SecureApi";
            var jwtAudience = "SecureApiUser";
            var jwtDurationInDays = 30;
            var jwtKey = "ZAgxvT1MyNPUVMjzdbwQQOAvUAvHSgWD3EMY0vTZwik=";

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.Now.AddDays(jwtDurationInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
       





    }
}
