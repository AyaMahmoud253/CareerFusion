﻿using Microsoft.AspNetCore.Mvc;
using Web_API.Models;
using Web_API.services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
    }
}