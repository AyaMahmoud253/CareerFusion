using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public interface IPostService
    {
        Task<IEnumerable<Post>> GetPostsAsync();
        Task<Post> GetPostByIdAsync(int postId);
        Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId); // New method

        Task<Post> CreatePostAsync(string userId, PostModel content);

    }
}
