using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public interface IPostService
    {
        Task<IEnumerable<PostWithUserDetailsDto>> GetPostsAsync();
        Task<Post> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostWithUserDetailsDto>> GetPostsByUserIdAsync(string userId); // Updated method

        Task<Post> CreatePostAsync(string userId, PostModel content);

    }
}
