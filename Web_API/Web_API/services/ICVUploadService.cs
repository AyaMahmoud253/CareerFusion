using System.Threading.Tasks;

namespace Web_API.Services
{
    public interface ICVUploadService // Changed interface name to ICVUploadService
    {
        Task<string> UploadFileAsync(int postId, IFormFile file);
        Task<IEnumerable<string>> GetCVPathsForPostAsync(int postId); // New method

    }
}
