using Web_API.Models;

namespace Web_API.services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);

    }
}
