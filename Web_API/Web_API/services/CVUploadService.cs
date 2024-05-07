using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;

namespace Web_API.Services
{
    public class CVUploadService : ICVUploadService
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CVUploadService(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor)
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
                var baseUrl = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                string urlPath = $"{baseUrl}/post-uploads/{uniqueFileName}";
                var postCV = new PostCV
                {
                    PostId = postId,
                    FilePath = urlPath
                };

                _context.PostCVs.Add(postCV);
                await _context.SaveChangesAsync();

                return "File uploaded successfully.";
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while uploading the file: {ex.Message}");
            }
        }



        public async Task<IEnumerable<string>> GetCVPathsForPostAsync(int postId)
        {
            var cvPaths = await _context.PostCVs
                .Where(cv => cv.PostId == postId)
                .Select(cv => cv.FilePath)
                .ToListAsync();

            return cvPaths;
        }
    }
}
