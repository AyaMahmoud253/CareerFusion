using Web_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web_API.services
{
    public interface IUserService
    {
        Task<AuthModel> GetUserByIdAsync(string userId);
        Task<AuthModel> GetUserWithRolesAsync(string userId);
        Task<string> DeleteUserAsync(string userId);
        Task<IEnumerable<AuthModel>> GetAllUsersAsync();
        Task<string> UpdateUserAsync(string userId, UpdateUserModel model);
    }
}
