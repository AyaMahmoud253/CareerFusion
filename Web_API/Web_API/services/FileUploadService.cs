using Microsoft.AspNetCore.Hosting; // Add this using directive
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

using Web_API.Models;
using Microsoft.EntityFrameworkCore;

namespace Web_API.Services
{
    public class FileUploadService : IFileUploadService
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;


        public FileUploadService(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> UploadFileAsync(int postId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is required.");
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                throw new ArgumentException($"Post with ID {postId} not found.");
            }

            string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "post-uploads");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            try
            {
                string uniqueFileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(file.FileName)}";
                string filePath = Path.Combine(uploadDirectory, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Store the URL path in the database
                // Get the base URL of the application
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";

                // Construct the file URL
                string urlPath = $"{baseUrl}/post-uploads/{uniqueFileName}";
                var postFile = new PostFile
                {
                    PostId = postId,
                    FilePath = urlPath // Store URL path instead of file path
                };

                _context.PostFiles.Add(postFile);
                await _context.SaveChangesAsync();

                return "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while uploading the file: {ex.Message}");
            }
        }

        public async Task<string> GetFileUrlAsync(int fileId)
        {
            var file = await _context.PostFiles.FindAsync(fileId);
            if (file == null)
            {
                throw new ArgumentException($"File with ID {fileId} not found.");
            }

            return file.FilePath;
        }


    }
}
