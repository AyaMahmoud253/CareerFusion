using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Models;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PostWithUserDetailsDto>>> GetPosts()
        {
            var posts = await _postService.GetPostsAsync();
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<ActionResult<Post>> GetPost(int postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpGet("HrPost/{userId}")]
        public async Task<ActionResult<IEnumerable<PostWithUserDetailsDto>>> GetPostsByUserId(string userId)
        {
            var posts = await _postService.GetPostsByUserIdAsync(userId);
            return Ok(posts);
        }


        [HttpPost("add/{userId}")]
        public async Task<IActionResult> CreatePost([FromRoute] string userId, [FromBody] PostModel post)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Use the PostService to create the post
                var createdPost = await _postService.CreatePostAsync(userId, post);

                // Return the created post with a 201 Created status
                return CreatedAtAction(nameof(GetPost), new { postId = createdPost.PostId }, createdPost);
            }
            catch (UnauthorizedAccessException ex)
            {
                // Return 401 Unauthorized if the user is not authorized to create a post
                return Unauthorized(ex.Message);
            }
        }







    }
}
