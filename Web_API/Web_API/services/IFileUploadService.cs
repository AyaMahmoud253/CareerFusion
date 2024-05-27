using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Web_API.Models;

namespace Web_API.Services
{
    public interface IFileUploadService
    {
        Task<int> UploadFileAsync(int postId, IFormFile file);
        Task<string> GetFileUrlAsync(int fileId);

    }
}
