using Web_API.Models;

namespace Web_API.services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<AuthModel> ConfirmEmailAsync(string userId, string token);
        Task<AuthModel> ForgetPasswordAsync(string email);
        Task<AuthModel> ResetPasswordAsync(ResetPasswordViewModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
     

    }
}
