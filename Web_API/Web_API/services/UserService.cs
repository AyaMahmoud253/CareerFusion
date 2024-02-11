using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;


        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
        }
        public async Task<AuthModel> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return new AuthModel { Message = "User not found!" };

            
            var authModel = new AuthModel
            {
                Email = user.Email,
                UserName = user.UserName,
                IsAuthenticated = true,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,
                // Add more properties as needed
            };

            return authModel;
        }
        public async Task<IEnumerable<AuthModel>> GetAllUsersAsync()
        {
            var users = await _userManager.Users.ToListAsync();

           
            var authModels = users.Select(user => new AuthModel
            {
                Email = user.Email,
                UserName = user.UserName,
                UserId = user.Id,
                IsAuthenticated = true,
                PhoneNumber = user.PhoneNumber,
                FullName = user.FullName,

                // Add more properties as needed
            });

            return authModels;
        }

        public async Task<AuthModel> GetUserWithRolesAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return new AuthModel { Message = "User not found!" };
            }

            var roles = await _userManager.GetRolesAsync(user);

            var authModel = new AuthModel
            {
             
                Roles = roles.ToList(),
                // Add more properties as needed
            };

            return authModel;
        }



        public async Task<string> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                // Handle scenario when user is not found
                return "User not found";
            }

            var result = await _userManager.DeleteAsync(user);

            return result.Succeeded ? string.Empty : "Failed to delete user";
        }

        public async Task<string> UpdateUserAsync(string userId, UpdateUserModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return "User not found";
            }

            // Update the properties only if they are provided
            if (!string.IsNullOrWhiteSpace(model.UserName))
            {
                user.UserName = model.UserName;
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                user.Email = model.Email;
            }

            if (!string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrWhiteSpace(model.FullName))
            {
                user.FullName = model.FullName; // Assuming FullName is a property of ApplicationUser
            }

            // Save the changes
            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded ? string.Empty : "Failed to update user";
        }

    }
}