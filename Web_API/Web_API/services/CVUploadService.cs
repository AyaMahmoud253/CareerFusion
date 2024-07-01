using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
using Microsoft.AspNetCore.Identity;
using Web_API.Controllers;
using Web_API.services;
using OfficeOpenXml;
using Microsoft.AspNetCore.SignalR;
using Web_API.Hubs;

namespace Web_API.Services
{
    public class CVUploadService : ICVUploadService
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<CVUploadService> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;





        public CVUploadService(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, IHttpContextAccessor httpContextAccessor, UserManager<ApplicationUser> userManager, ILogger<CVUploadService> logger, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _logger = logger;
            _hubContext = hubContext;

        }

        public async Task<string> UploadFileAsync(int postId, string userId, IFormFile file)
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
                string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
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
                    FilePath = urlPath,
                    UserId = userId // Set the UserId
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
        public async Task<bool> UpdateScreeningStatusAsync(int postId, List<string> emails)
        {
            try
            {
                // Fetch all users based on emails
                var users = await _userManager.Users
                    .Where(u => emails.Contains(u.Email))
                    .ToListAsync();

                if (users == null || !users.Any())
                {
                    throw new Exception("No users found for the provided emails.");
                }

                var userIds = users.Select(u => u.Id).ToList();

                // Fetch all PostCVs for the given postId and user ids, including related entities
                var postCVs = await _context.PostCVs
                    .Include(cv => cv.UploadedPost)
                    .ThenInclude(post => post.User) // Ensure User is loaded
                    .Where(cv => cv.PostId == postId && userIds.Contains(cv.UserId))
                    .ToListAsync();

                if (postCVs == null || !postCVs.Any())
                {
                    throw new Exception("No PostCVs found for the provided users and post ID.");
                }

                // Update PassedScreening and UserId for matched PostCVs
                foreach (var postCV in postCVs)
                {
                    postCV.PassedScreening = true;

                    // Check if UploadedPost and User are not null
                    if (postCV.UploadedPost?.User != null)
                    {
                        // Find the matching user by email
                        var user = users.FirstOrDefault(u => u.Email == postCV.UploadedPost.User.Email);

                        if (user != null)
                        {
                            postCV.UserId = user.Id; // Update UserId in PostCV
                        }
                    }
                }

                await _context.SaveChangesAsync();

                return true;  // Success
            }
            catch (Exception ex)
            {
                // Handle exception as needed (logging, etc.)
                throw new Exception($"Failed to update screening status: {ex.Message}");
            }
        }




        public async Task<IEnumerable<object>> GetScreenedUsersAsync(int postId)
        {
            try
            {
                var records = await _context.PostCVs
                    .Include(cv => cv.UploadedPost)
                    .Where(cv => cv.PassedScreening && cv.PostId == postId)
                    .Select(cv => new
                    {
                        cv.PostCVId,
                        cv.PostId,
                        cv.UserId,
                        UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                        UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                        cv.FilePath
                    })
                    .ToListAsync();

                return records;
            }
            catch (Exception ex)
            {
                // Handle exception as needed (logging, etc.)
                throw new Exception($"Failed to retrieve records: {ex.Message}");
            }
        }

        public async Task<ServiceResult> SetTelephoneInterviewDateForPostAsync(int id, int postId, DateTime interviewDate)
        {
            try
            {
                var postCV = await _context.PostCVs.FindAsync(id);
                if (postCV == null || postCV.PostId != postId)
                {
                    return new ServiceResult { Success = false, Message = $"PostCV with ID '{id}' not found for the specified post ID '{postId}'." };
                }

                var post = await _context.Posts.FindAsync(postId);
                if (post == null)
                {
                    return new ServiceResult { Success = false, Message = $"Post with ID '{postId}' not found." };
                }

                var user = await _userManager.FindByIdAsync(post.UserId);
                if (user == null)
                {
                    return new ServiceResult { Success = false, Message = $"User associated with Post ID '{postId}' not found." };
                }

                postCV.TelephoneInterviewDate = interviewDate;

                var hrName = user.FullName ?? "HR";

                var notification = new Notification
                {
                    UserId = postCV.UserId,
                    Message = $"Your telephone interview is scheduled for {interviewDate:yyyy-MM-dd} by HR {hrName}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                _context.PostCVs.Update(postCV); // Ensure the changes to postCV are tracked
                await _context.SaveChangesAsync();

                // Send SignalR notification
                if (NotificationHub.UserConnections.TryGetValue(postCV.UserId, out string connectionId))
                {
                    await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveNotification", notification.Message);
                }

                return new ServiceResult { Success = true, Message = $"Interview date set for Post ID '{postId}' and CV ID '{id}'." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> GetTelephoneInterviewDateForPostAsync(int id, int postId)
        {
            try
            {
                var postCV = await _context.PostCVs.FindAsync(id);
                if (postCV == null || postCV.PostId != postId)
                {
                    return new ServiceResult { Success = false, Message = $"PostCV with ID '{id}' not found for the specified post ID '{postId}'." };
                }

                var interviewDate = postCV.TelephoneInterviewDate;
                return new ServiceResult { Success = true, Data = interviewDate };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }



        public async Task<ServiceResult> ToggleTelephoneInterviewForPostCVAsync(int postId, int postCVId)
        {
            var postCV = await _context.PostCVs
                .Where(cv => cv.PostId == postId && cv.PostCVId == postCVId)
                .FirstOrDefaultAsync();

            if (postCV == null)
            {
                return new ServiceResult { Success = false, Message = "Post CV not found." };
            }

            postCV.IsTelephoneInterviewPassed = !postCV.IsTelephoneInterviewPassed;

            try
            {
                await _context.SaveChangesAsync();

                // Send notification to the user
                var notification = new Notification
                {
                    UserId = postCV.UserId,
                    Message = "Congratulations! You have passed the telephone interview.",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send SignalR notification
                await _hubContext.Clients.User(postCV.UserId)
                    .SendAsync("ReceiveNotification", notification.Message);

                return new ServiceResult { Success = true, Message = $"Telephone interview status toggled for Post CV ID {postCVId}." };
            }
            catch (Exception ex)
            {
                // Handle exception as needed
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<object>> GetRecordsWithTelephoneInterviewPassedAsync(int postId)
        {
            var records = await _context.PostCVs
                .Where(cv => cv.IsTelephoneInterviewPassed && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.PostCVId,
                    cv.PostId,
                    cv.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                    UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                    cv.FilePath
                }).ToListAsync();

            return records;
        }

        public async Task<MemoryStream> ExportRecordsWithTelephoneInterviewPassedToExcelAsync(int postId)
        {
            var records = await _context.PostCVs
                .Where(cv => cv.IsTelephoneInterviewPassed && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.PostCVId,
                    cv.PostId,
                    cv.UserId,
                    User = _context.Users
                        .Where(u => u.Id == cv.UserId)
                        .Select(u => new
                        {
                            u.FullName,
                            u.Email,
                            u.PhoneNumber
                        }).FirstOrDefault(),
                    // You can include other fields from the Post model if needed, but not Title
                    // PostInfo = _context.Posts.Where(p => p.PostId == cv.PostId).FirstOrDefault()
                }).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Records");

                // Add headers
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Phone Number";
                // Add other headers if you included more fields

                // Add records
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].User?.FullName;
                    worksheet.Cells[i + 2, 2].Value = records[i].User?.Email;
                    worksheet.Cells[i + 2, 3].Value = records[i].User?.PhoneNumber;
                    // Add other fields if you included more
                }

                // Auto-fit columns for all cells
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return stream;
            }
        }

        public async Task<ServiceResult> SetTechnicalInterviewDateAsync(int postCVId, int postId, DateTime? technicalAssessmentDate, DateTime? physicalInterviewDate)
        {
            var postCV = await _context.PostCVs.FindAsync(postCVId);

            if (postCV == null || postCV.PostId != postId)
            {
                return new ServiceResult { Success = false, Message = "Post CV not found for the specified post." };
            }

            if (!postCV.IsTelephoneInterviewPassed)
            {
                return new ServiceResult { Success = false, Message = "Telephone interview not passed. Cannot set interview dates." };
            }

            try
            {
                var post = await _context.Posts.FindAsync(postCV.PostId);
                if (post == null)
                {
                    return new ServiceResult { Success = false, Message = "Post not found." };
                }

                var user2 = await _userManager.FindByIdAsync(post.UserId);
                if (user2 == null)
                {
                    return new ServiceResult { Success = false, Message = "HR not found." };
                }
                var HR = user2.FullName;

                var user = await _userManager.FindByIdAsync(postCV.UserId);
                if (user != null)
                {
                    if (technicalAssessmentDate.HasValue)
                    {
                        postCV.TechnicalAssessmentDate = technicalAssessmentDate;
                        var notificationTechnical = new Notification
                        {
                            UserId = user.Id,
                            Message = $"Your technical assessment is scheduled for {technicalAssessmentDate:yyyy-MM-dd} by HR {HR}",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Notifications.Add(notificationTechnical);

                        // Send SignalR notification
                        await _hubContext.Clients.User(user.Id)
                            .SendAsync("ReceiveNotification", notificationTechnical.Message);
                    }

                    if (physicalInterviewDate.HasValue)
                    {
                        postCV.PhysicalInterviewDate = physicalInterviewDate;
                        var notificationPhysical = new Notification
                        {
                            UserId = user.Id,
                            Message = $"Your physical interview is scheduled for {physicalInterviewDate:yyyy-MM-dd} by HR {HR}",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.Notifications.Add(notificationPhysical);

                        // Send SignalR notification
                        await _hubContext.Clients.User(user.Id)
                            .SendAsync("ReceiveNotification", notificationPhysical.Message);
                    }
                    _context.PostCVs.Update(postCV);
                    await _context.SaveChangesAsync();
                }

                return new ServiceResult { Success = true, Message = $"Interview dates set for Post CV ID {postCVId}." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        // Service to get technical assessment data
        public async Task<ServiceResult> GetTechnicalAssessmentDateAsync(int postCVId, int postId)
        {
            var postCV = await _context.PostCVs
                .Where(cv => cv.PostCVId == postCVId && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.TechnicalAssessmentDate
                })
                .FirstOrDefaultAsync();

            if (postCV == null)
            {
                return new ServiceResult { Success = false, Message = "Post CV not found for the specified post." };
            }

            return new ServiceResult { Success = true, Data = postCV.TechnicalAssessmentDate };
        }

        // Service to get physical interview date
        public async Task<ServiceResult> GetPhysicalInterviewDateAsync(int postCVId, int postId)
        {
            var postCV = await _context.PostCVs
                .Where(cv => cv.PostCVId == postCVId && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.PhysicalInterviewDate
                })
                .FirstOrDefaultAsync();

            if (postCV == null)
            {
                return new ServiceResult { Success = false, Message = "Post CV not found for the specified post." };
            }

            return new ServiceResult { Success = true, Data = postCV.PhysicalInterviewDate };
        }

        public async Task<ServiceResult> ToggleTechnicalInterviewForPostCVAsync(int postId, int postCVId, bool passed, string hrMessage)
        {
            var postCV = await _context.PostCVs
                .Where(cv => cv.PostId == postId && cv.PostCVId == postCVId)
                .FirstOrDefaultAsync();

            if (postCV == null)
            {
                return new ServiceResult { Success = false, Message = "Post CV not found." };
            }

            postCV.IsTechnicalInterviewPassed = passed;

            try
            {
                await _context.SaveChangesAsync();

                // Send customized notification to the user
                var notification = new Notification
                {
                    UserId = postCV.UserId,
                    Message = passed ? $"Congratulations! {hrMessage}" : $"We're sorry to inform you that you did not pass the technical interview. {hrMessage}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Send SignalR notification
                await _hubContext.Clients.User(postCV.UserId)
                    .SendAsync("ReceiveNotification", notification.Message);

                return new ServiceResult { Success = true, Message = $"Technical interview status toggled for Post CV ID {postCVId}." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<IEnumerable<object>> GetRecordsWithTechnicalInterviewPassedAsync(int postId)
        {
            var records = await _context.PostCVs
                .Where(cv => cv.IsTechnicalInterviewPassed && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.PostCVId,
                    cv.PostId,
                    cv.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                    UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                    cv.FilePath
                }).ToListAsync();

            return records;
        }

        public async Task<MemoryStream> ExportRecordsWithTechnicalInterviewPassedToExcelAsync(int postId)
        {
            var records = await _context.PostCVs
                .Where(cv => cv.IsTechnicalInterviewPassed && cv.PostId == postId)
                .Select(cv => new
                {
                    cv.PostCVId,
                    cv.PostId,
                    cv.UserId,
                    User = _context.Users
                        .Where(u => u.Id == cv.UserId)
                        .Select(u => new
                        {
                            u.FullName,
                            u.Email,
                            u.PhoneNumber
                        }).FirstOrDefault(),
                    // You can include other fields from the Post model if needed
                }).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Records");

                // Add headers
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Phone Number";
                // Add other headers if you included more fields

                // Add records
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].User?.FullName;
                    worksheet.Cells[i + 2, 2].Value = records[i].User?.Email;
                    worksheet.Cells[i + 2, 3].Value = records[i].User?.PhoneNumber;
                    // Add other fields if you included more
                }

                // Auto-fit columns for all cells
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return stream;
            }
        }

        public async Task<List<object>> GetUsersPassedTechnicalInterviewForHRPostsAsync(string hrUserId)
        {
            try
            {
                // Find all posts posted by the HR user
                var postIds = await _context.Posts
                    .Where(p => p.UserId == hrUserId)
                    .Select(p => p.PostId)
                    .ToListAsync();

                if (postIds == null || !postIds.Any())
                {
                    throw new ArgumentException($"No posts found for HR user with ID {hrUserId}.");
                }

                // Find post CVs that have passed the technical interview for these posts
                var records = await _context.PostCVs
                    .Where(cv => postIds.Contains(cv.PostId) && cv.IsTechnicalInterviewPassed)
                    .Select(cv => new
                    {
                        cv.PostCVId,
                        cv.PostId,
                        cv.UserId,
                        UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                        UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                        cv.FilePath
                    }).ToListAsync<object>();

                return records;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred in CVUploadService: {ex.Message}", ex);
            }
        }






    }
}
