﻿using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public class PostService : IPostService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostService(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<PostWithUserDetailsDto>> GetPostsAsync()
        {
            var posts = await _context.Posts
                .Include(p => p.User) // Ensure the User entity is included
                .ToListAsync();

            var postDtos = posts.Select(post => new PostWithUserDetailsDto
            {
                PostId = post.PostId,
                Content = post.Content,
                CreatedAt = post.CreatedAt,
                UserId = post.User.Id,
                UserFullName = post.User.FullName,
                UserEmail = post.User.Email,
                UserProfilePicturePath = post.User.ProfilePicturePath
            });

            return postDtos;
        }
        public async Task<Post> GetPostByIdAsync(int postId)
        {
            return await _context.Posts.FindAsync(postId);
        }

        public async Task<IEnumerable<Post>> GetPostsByUserIdAsync(string userId)
        {
            return await _context.Posts
                .Where(p => p.UserId == userId)
                .ToListAsync();
        }

        public async Task<Post> CreatePostAsync(string userId, PostModel content)
        {
            var user = await _userManager.FindByIdAsync(userId);

            // Check if user has the HR role
            if (user == null || !await _userManager.IsInRoleAsync(user, "HR"))
            {
                throw new UnauthorizedAccessException("User is not authorized to create a post.");
            }

            // Create a new Post object
            var post = new Post
            {
                UserId = userId,
                Content = content.Content,
                CreatedAt = DateTime.UtcNow
            };

            // Add the post to the database
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return post;
        }


    }
}