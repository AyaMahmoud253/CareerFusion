using Microsoft.AspNetCore.Mvc;
using Web_API.Models;
using Web_API.services;
using System.Threading.Tasks;

namespace Web_API.controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private IMailService _mailService;
        private IConfiguration _configuration;
        public AuthController(IAuthService authService, IMailService mailService, IConfiguration configuration)
        {
            _authService = authService;
            _mailService = mailService;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            // Include both token and user ID in the response
            return Ok(new { Token = result.Token, UserId = result.UserId });
        }


        [HttpPost("token")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(result.Message);

            // Send an email notification about the new login
            await _mailService.SendEmailAsync(model.Email, "New login", $"<h1>Hey!, new login to your account noticed</h1><p>New login to your account at {DateTime.Now}</p>");

            // Include token, user ID, and roles in the response
            return Ok(new
            {
                Token = result.Token,
                UserId = result.UserId,
                Roles = result.Roles // Include roles
            });
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return NotFound();
            var result = await _authService.ConfirmEmailAsync(userId, token);

            if (result.IsAuthenticated)
            {
                return Redirect($"{_configuration["AppUrl"]}/ConfirmEmail.html");
            }

            return BadRequest(result);
        }

        [HttpPost("ForgetPassword")]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
                return NotFound();

            var result = await _authService.ForgetPasswordAsync(email);

            if (result.IsAuthenticated)
                return Ok(result); // 200

            return BadRequest(result); // 400
        }

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromForm] ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authService.ResetPasswordAsync(model);

                if (result.IsAuthenticated)
                    return Ok(result);

                return BadRequest(result);


            }
            return BadRequest("Some properties are not valid");

        }

        [HttpPost("addrole")]
        public async Task<IActionResult> AddRoleAsync([FromBody] AddRoleModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.AddRoleAsync(model);

            if (!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok("Role added successfully!");//can return model
        }
    }
}
