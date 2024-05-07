using Azure.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Services
{
    public class PictureUploadService : IPictureUploadService
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PictureUploadService(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadPictureAsync(int postId, IFormFile picture)
        {
            if (picture == null || picture.Length == 0)
            {
                throw new ArgumentException("Picture is required.");
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                throw new ArgumentException($"Post with ID {postId} not found.");
            }

            string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "post-pictures");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            try
            {
                string uniqueFileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(picture.FileName)}";
                string filePath = Path.Combine(uploadDirectory, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await picture.CopyToAsync(stream);
                }

                // Store the URL path in the database
                // Construct the URL path
                string baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                string urlPath = $"{baseUrl}/post-pictures/{uniqueFileName}";

                var postPicture = new PostPicture
                {
                    PostId = postId,
                    PicturePath = urlPath // Store URL path instead of file path
                };

                _context.PicturePosts.Add(postPicture);
                await _context.SaveChangesAsync();

                return "Picture uploaded successfully.";
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while uploading the picture: {ex.Message}");
            }
        }

        public async Task<string> GetPicturePathAsync(int pictureId)
        {
            var picture = await _context.PicturePosts.FindAsync(pictureId);
            if (picture == null)
            {
                throw new ArgumentException($"Picture with ID {pictureId} not found.");
            }

            return picture.PicturePath;
        }
    }
}
