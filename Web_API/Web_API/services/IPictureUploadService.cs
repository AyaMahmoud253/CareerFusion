using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Web_API.Services
{
    public interface IPictureUploadService
    {
        Task<string> UploadPictureAsync(int postId, IFormFile picture);
        Task<string> GetPicturePathAsync(int pictureId);

    }
}
