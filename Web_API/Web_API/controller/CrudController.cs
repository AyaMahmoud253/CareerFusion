using Microsoft.AspNetCore.Mvc;
using Web_API.Models;
using Web_API.services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    //[Authorize(Policy = "AdminOnly")]
    public class CrudController : ControllerBase
    {
        private readonly IUserService _userService;

        public CrudController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _userService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            return Ok(user);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();

            if (users == null || !users.Any())
            {
                return NotFound("No users found");
            }

            return Ok(users);
        }

        [HttpGet("{userId}/roles")]
        public async Task<IActionResult> GetUserRoles(string userId)
        {
            var userWithRoles = await _userService.GetUserWithRolesAsync(userId);

            if (userWithRoles == null)
            {
                return NotFound("User not found");
            }

            return Ok(userWithRoles);
        }

        [HttpDelete("userDel/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var result = await _userService.DeleteUserAsync(userId);

            if (!string.IsNullOrEmpty(result))
            {
                return NotFound(result);
            }

            return Ok("User deleted successfully");
        }

        [HttpPut("updateUser/{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserModel model)
        {
            if (model == null)
            {
                return BadRequest("Invalid user data");
            }

            var result = await _userService.UpdateUserAsync(userId, model);

            if (!string.IsNullOrEmpty(result))
            {
                return BadRequest(result);
            }

            return Ok("User updated successfully");
        }
    }
}
