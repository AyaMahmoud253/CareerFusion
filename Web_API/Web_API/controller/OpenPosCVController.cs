﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;
using Web_API.services;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Collections.Generic;
using OfficeOpenXml;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.SignalR;
using Web_API.Hubs;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class OpenPosCVController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OpenPosCVController> _logger;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IEmailService _emailService;





        public OpenPosCVController(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager,
            IConfiguration configuration, ILogger<OpenPosCVController> logger,IHubContext<NotificationHub> hubContext, IEmailService emailService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
            _hubContext = hubContext;
            _emailService = emailService;



        }
        [HttpGet("{hrUserId}/technical-interview-passed")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersPassedTechnicalInterviewForHR(string hrUserId)
        {
            try
            {
                // Find job forms added by the specified HR user
                var jobForms = await _context.JobForms
                    .Where(jf => jf.UserId == hrUserId)
                    .ToListAsync();

                if (jobForms == null || !jobForms.Any())
                {
                    return NotFound(new { Message = $"No job forms found for HR user with ID {hrUserId}." });
                }

                // Get the IDs of these job forms
                var jobFormIds = jobForms.Select(jf => jf.Id).ToList();

                // Find job form CVs that have passed the technical interview
                var records = await _context.JobFormCVs
                    .Where(cv => jobFormIds.Contains(cv.JobFormId) && cv.IsTechnicalInterviewPassed) // Assuming you have a flag for technical interview status
                    .Select(cv => new
                    {
                        cv.Id,
                        cv.JobFormId,
                        cv.UserId,
                        UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                        UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                        cv.FilePath
                    }).ToListAsync();

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPost("{jobFormId}/upload-cv")]
        public async Task<IActionResult> UploadCVForJobForm(int jobFormId, string userId, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest("CV file is required.");
            }

            var jobForm = await _context.JobForms.FindAsync(jobFormId);
            if (jobForm == null)
            {
                return NotFound($"Job form with ID {jobFormId} not found.");
            }

            try
            {
                // Get the next available CV ID
                int nextCvId = await GetNextCVId();

                string uniqueFileName = $"{nextCvId}_{Path.GetFileName(cvFile.FileName)}";
                string filePath = Path.Combine(_hostingEnvironment.WebRootPath, "cv-uploads", uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await cvFile.CopyToAsync(stream);
                }

                // Store the URL path in the database
                string urlPath = $"{Request.Scheme}://{Request.Host}/cv-uploads/{uniqueFileName}";

                var jobFormCV = new JobFormCV
                {
                    JobFormId = jobFormId,
                    FilePath = urlPath,
                    UserId = userId // Associate the CV with the user
                };

                _context.JobFormCVs.Add(jobFormCV);
                await _context.SaveChangesAsync();

                // Return the CV ID
                return Ok(new { Id = jobFormCV.Id, Message = "CV uploaded successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while uploading the CV: {ex.Message}");
            }
        }

        private async Task<int> GetNextCVId()
        {
            // Get the highest CV ID from the database
            var highestCvId = await _context.JobFormCVs.OrderByDescending(cv => cv.Id).FirstOrDefaultAsync();

            // If no CVs are found, start from CV ID 1
            return highestCvId == null ? 1 : highestCvId.Id + 1;
        }


        [HttpGet("{jobFormId}/download-cvs")]
        public async Task<IActionResult> DownloadCVsForJobForm(int jobFormId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobFormCVs)
                .FirstOrDefaultAsync(jf => jf.Id == jobFormId);

            if (jobForm == null)
            {
                return NotFound($"Job form with ID {jobFormId} not found.");
            }

            var cvFiles = jobForm.JobFormCVs.Select(cv => new
            {
                cv.FilePath,
                FileName = Path.GetFileName(cv.FilePath)
            }).ToList();

            // Create a temporary directory for storing the files to be zipped
            var tempDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
            Directory.CreateDirectory(tempDirectory);

            try
            {
                // Copy each file to the temp directory
                foreach (var cvFile in cvFiles)
                {
                    var sourceFilePath = GetPhysicalFilePath(cvFile.FilePath);
                    var destFilePath = Path.Combine(tempDirectory, cvFile.FileName);

                    if (!System.IO.File.Exists(sourceFilePath))
                    {
                        return NotFound($"File not found: {cvFile.FilePath}");
                    }

                    System.IO.File.Copy(sourceFilePath, destFilePath, true);
                }

                // Determine the zip file path
                var zipFileName = $"jobform-{jobFormId}.zip";
                var zipFilePath = Path.Combine(_hostingEnvironment.WebRootPath, zipFileName);

                // If the zip file already exists, delete it
                if (System.IO.File.Exists(zipFilePath))
                {
                    System.IO.File.Delete(zipFilePath);
                }

                // Zip all files in the temp directory
                ZipFile.CreateFromDirectory(tempDirectory, zipFilePath);

                // Cleanup: delete the temp directory
                Directory.Delete(tempDirectory, true);

                // Return the zip file
                var zipFileBytes = System.IO.File.ReadAllBytes(zipFilePath);
                return File(zipFileBytes, "application/zip", zipFileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while creating the zip file: {ex.Message}");
            }
        }

        // Helper method to get the physical file path from URL
        private string GetPhysicalFilePath(string urlPath)
        {
            var uri = new Uri(urlPath);
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, uri.AbsolutePath.TrimStart('/').Replace("/", "\\"));
            return filePath;
        }


        [HttpGet("{jobFormId}/cvs")]
        public async Task<ActionResult<IEnumerable<object>>> GetCVsForJobForm(int jobFormId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobFormCVs)
                .FirstOrDefaultAsync(jf => jf.Id == jobFormId);

            if (jobForm == null)
            {
                return NotFound($"Job form with ID {jobFormId} not found.");
            }

            // Convert file paths to URLs and include UserId
            var cvDetails = jobForm.JobFormCVs.Select(cv => new
            {
                cv.FilePath,
                cv.UserId
            }).ToList();

            return Ok(cvDetails);
        }

        [HttpGet("all-cvs")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllCVs()
        {
            var allCVs = await _context.JobFormCVs
                .Select(cv => new
                {
                    cv.JobFormId,
                    cv.UserId,
                    cv.FilePath
                }).ToListAsync();

            return Ok(allCVs);
        }


        [HttpGet("{userId}/contact-info")]
        public async Task<IActionResult> GetUserContactInfo(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var contactInfo = new
            {
                user.FullName,
                user.PhoneNumber,
                user.Email
            };

            return Ok(contactInfo);
        }
        [HttpGet("{jobFormId}/user-id")]
        public async Task<IActionResult> GetUserIdForJobForm(int jobFormId)
        {
            var jobForm = await _context.JobForms.FindAsync(jobFormId);

            if (jobForm == null)
            {
                return NotFound($"Job form with ID {jobFormId} not found.");
            }

            // Assuming JobForm has a UserId property, or you need to fetch it from related data
            var userId = jobForm.UserId;  // Adjust this based on your data structure
            var user = await _userManager.FindByIdAsync(userId);

            var fullName = user.FullName;

            return Ok(new { UserId = userId, FullName = fullName });
        }
        [HttpGet("{userId}/jobform-ids")]
        public async Task<IActionResult> GetJobFormIdsByUserId(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                var jobFormIds = await _context.JobForms
                    .Where(jf => jf.UserId == userId)
                    .Select(jf => jf.Id)
                    .ToListAsync();

                return Ok(jobFormIds);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpPut("update-scores-for-screened/{jobFormId}")]
        public async Task<IActionResult> UpdateScores(int jobFormId, [FromBody] List<string> emails)
        {
            try
            {
                var users = await _userManager.Users
                    .Where(u => emails.Contains(u.Email))
                    .ToListAsync();

                if (users == null || !users.Any())
                {
                    return NotFound(new ServiceResult { Success = false, Message = "No users found for the provided emails." });
                }

                var userIds = users.Select(u => u.Id).ToList();

                var jobFormCVs = await _context.JobFormCVs
                    .Where(j => userIds.Contains(j.UserId) && j.JobFormId == jobFormId)
                    .ToListAsync();

                if (jobFormCVs == null || !jobFormCVs.Any())
                {
                    return NotFound(new ServiceResult { Success = false, Message = "No JobFormCVs found for the provided users and job form ID." });
                }

                var updatedJobFormCVIds = new List<int>();

                foreach (var jobFormCV in jobFormCVs)
                {
                    jobFormCV.IsScoreAbove70 = true;
                    updatedJobFormCVIds.Add(jobFormCV.Id);
                }

                await _context.SaveChangesAsync();

                return Ok(new ServiceResult { Success = true, Message = "Scores updated successfully.", Data = updatedJobFormCVIds });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{id}/jobform/{jobFormId}/set-telephone-interview-date")]
        public async Task<IActionResult> SetTelephoneInterviewDate(int id, int jobFormId, DateTime interviewDate)
        {
            try
            {
                var jobFormCV = await _context.JobFormCVs.FindAsync(id);
                if (jobFormCV == null || jobFormCV.JobFormId != jobFormId)
                {
                    return NotFound(new ServiceResult { Success = false, Message = $"JobFormCV with ID '{id}' not found for the specified job form ID '{jobFormId}'." });
                }

                var jobForm = await _context.JobForms.FindAsync(jobFormId);
                if (jobForm == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = $"JobForm with ID '{jobFormId}' not found." });
                }

                var user = await _userManager.FindByIdAsync(jobForm.UserId);
                if (user == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = $"User associated with JobForm ID '{jobFormId}' not found." });
                }

                // Set the telephone interview date
                jobFormCV.TelephoneInterviewDate = interviewDate;

                var hrName = user.FullName ?? "HR";

                // Create a notification for the user
                var notification = new Notification
                {
                    UserId = jobFormCV.UserId,
                    Message = $"Your telephone interview is scheduled for {interviewDate:yyyy-MM-dd} by HR {hrName}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                _context.JobFormCVs.Update(jobFormCV); // Ensure the changes to jobFormCV are tracked
                await _context.SaveChangesAsync();

                // Send the notification via SignalR
                await _hubContext.Clients.User(jobFormCV.UserId)
                    .SendAsync("ReceiveNotification", notification.Message);

                // Get user details
                var notificationUser = await _userManager.FindByIdAsync(jobFormCV.UserId);
                if (notificationUser != null)
                {
                    // Send email notification
                    await _emailService.SendEmailAsync(
                        to: notificationUser.Email,
                        subject: "Telephone Interview Scheduled",
                        htmlContent: $"<p>{notification.Message}</p>",
                        plainTextContent: notification.Message
                    );
                }


                return Ok(new ServiceResult { Success = true, Message = $"Interview date set for JobForm ID '{jobFormId}' and CV ID '{id}'." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/jobform/{jobFormId}/get-telephone-interview-date")]
        public async Task<IActionResult> GetTelephoneInterviewDate(int id, int jobFormId)
        {
            try
            {
                var jobFormCV = await _context.JobFormCVs.FindAsync(id);
                if (jobFormCV == null || jobFormCV.JobFormId != jobFormId)
                {
                    return NotFound(new ServiceResult { Success = false, Message = $"JobFormCV with ID '{id}' not found for the specified job form ID '{jobFormId}'." });
                }

                return Ok(new ServiceResult { Success = true, Data = jobFormCV.TelephoneInterviewDate });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }



        [HttpPut("{jobFormId}/{cvId}/toggle-telephone-interview")]
        public async Task<IActionResult> ToggleTelephoneInterview(int jobFormId, int cvId)
        {
            var jobFormCV = await _context.JobFormCVs
                .Where(cv => cv.JobFormId == jobFormId && cv.Id == cvId)
                .FirstOrDefaultAsync();

            if (jobFormCV == null)
            {
                return NotFound(new ServiceResult { Success = false, Message = "Job form CV not found." });
            }

            jobFormCV.isTelephoneInterviewPassed = !jobFormCV.isTelephoneInterviewPassed;

            try
            {
                if (jobFormCV.isTelephoneInterviewPassed)
                {
                    var notification = new Notification
                    {
                        UserId = jobFormCV.UserId,
                        Message = "Congratulations, you have passed the telephone interview!",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);

                    // Send the notification via SignalR
                    await _hubContext.Clients.User(jobFormCV.UserId)
                        .SendAsync("ReceiveNotification", notification.Message);

                    // Get user details
                    var user = await _userManager.FindByIdAsync(jobFormCV.UserId);
                    if (user != null)
                    {
                        // Send email notification
                        await _emailService.SendEmailAsync(
                            to: user.Email,
                            subject: "Telephone Interview Passed",
                            htmlContent: $"<p>{notification.Message}</p>",
                            plainTextContent: notification.Message
                        );
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new ServiceResult { Success = true, Message = $"Telephone interview status toggled for CV ID {cvId}." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPut("{id}/jobform/{jobFormId}/set-technical-interview-date")]
        public async Task<IActionResult> SetTechnicalInterviewDate(int id, int jobFormId, DateTime? technicalAssessmentDate, DateTime? physicalInterviewDate)
        {
            var jobFormCV = await _context.JobFormCVs.FindAsync(id);

            if (jobFormCV == null || jobFormCV.JobFormId != jobFormId)
            {
                return NotFound(new ServiceResult { Success = false, Message = "Job form CV not found for the specified job form." });
            }

            if (!jobFormCV.isTelephoneInterviewPassed)
            {
                return BadRequest(new ServiceResult { Success = false, Message = "Telephone interview not passed. Cannot set interview dates." });
            }

            try
            {
                var jobForm = await _context.JobForms.FindAsync(jobFormCV.JobFormId);
                if (jobForm == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = "Job form not found." });
                }

                var hrUser = await _userManager.FindByIdAsync(jobForm.UserId);
                if (hrUser == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = "HR not found." });
                }
                var hrName = hrUser.FullName;

                var user = await _userManager.FindByIdAsync(jobFormCV.UserId);
                if (user == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = "User not found." });
                }

                if (technicalAssessmentDate.HasValue)
                {
                    jobFormCV.TechnicalAssessmentDate = technicalAssessmentDate;
                    var notificationTechnical = new Notification
                    {
                        UserId = user.Id,
                        Message = $"Your technical assessment is scheduled for {technicalAssessmentDate:yyyy-MM-dd} by HR {hrName}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notificationTechnical);

                    // Send SignalR notification for technical assessment
                    await _hubContext.Clients.User(user.Id)
                        .SendAsync("ReceiveNotification", notificationTechnical.Message);

                    // Send email notification for technical assessment
                    await _emailService.SendEmailAsync(
                        to: user.Email,
                        subject: "Technical Assessment Scheduled",
                        htmlContent: $"<p>{notificationTechnical.Message}</p>",
                        plainTextContent: notificationTechnical.Message
                    );
                }

                if (physicalInterviewDate.HasValue)
                {
                    jobFormCV.PhysicalInterviewDate = physicalInterviewDate;
                    var notificationPhysical = new Notification
                    {
                        UserId = user.Id,
                        Message = $"Your physical interview is scheduled for {physicalInterviewDate:yyyy-MM-dd} by HR {hrName}",
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Notifications.Add(notificationPhysical);

                    // Send SignalR notification for physical interview
                    await _hubContext.Clients.User(user.Id)
                        .SendAsync("ReceiveNotification", notificationPhysical.Message);

                    // Send email notification for physical interview
                    await _emailService.SendEmailAsync(
                        to: user.Email,
                        subject: "Physical Interview Scheduled",
                        htmlContent: $"<p>{notificationPhysical.Message}</p>",
                        plainTextContent: notificationPhysical.Message
                    );
                }

                _context.JobFormCVs.Update(jobFormCV); // Ensure the changes to jobFormCV are tracked
                await _context.SaveChangesAsync();

                return Ok(new ServiceResult { Success = true, Message = $"Interview dates set for Job Form CV ID {id}." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{id}/jobform/{jobFormId}/technical-assessment-date")]
        public async Task<IActionResult> GetTechnicalAssessmentDate(int id, int jobFormId)
        {
            var jobFormCV = await _context.JobFormCVs.FindAsync(id);

            if (jobFormCV == null || jobFormCV.JobFormId != jobFormId)
            {
                return NotFound(new ServiceResult { Success = false, Message = "Job form CV not found for the specified job form." });
            }

            return Ok(new ServiceResult { Success = true, Data = jobFormCV.TechnicalAssessmentDate });
        }

        [HttpGet("{id}/jobform/{jobFormId}/physical-interview-date")]
        public async Task<IActionResult> GetPhysicalInterviewDate(int id, int jobFormId)
        {
            var jobFormCV = await _context.JobFormCVs.FindAsync(id);

            if (jobFormCV == null || jobFormCV.JobFormId != jobFormId)
            {
                return NotFound(new ServiceResult { Success = false, Message = "Job form CV not found for the specified job form." });
            }

            return Ok(new ServiceResult { Success = true, Data = jobFormCV.PhysicalInterviewDate });
        }



        [HttpGet("screened/{jobFormId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecordsWithScoreAbove70(int jobFormId)
        {
            var records = await _context.JobFormCVs
                .Where(cv => cv.IsScoreAbove70 && cv.JobFormId == jobFormId)
                .Select(cv => new
                {
                    cv.Id,
                    cv.JobFormId,
                    cv.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                    UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                    cv.FilePath
                }).ToListAsync();

            return Ok(records);
        }




        [HttpGet("telephone-interview-passed/{jobFormId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetRecordsWithTelephoneInterviewPassed(int jobFormId)
        {
            var records = await _context.JobFormCVs
                .Where(cv => cv.isTelephoneInterviewPassed && cv.JobFormId == jobFormId)
                .Select(cv => new
                {
                    cv.Id,
                    cv.JobFormId,
                    cv.UserId,
                    UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                    UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                    cv.FilePath
                }).ToListAsync();

            return Ok(records);
        }


        [HttpGet("{userId}/notifications")]
        public async Task<ActionResult<IEnumerable<Notification>>> GetUserNotifications(string userId)
        {
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }
        [HttpGet("export-telephone-interview-passed/{jobFormId}")]
        public async Task<IActionResult> ExportRecordsWithTelephoneInterviewPassedToExcel(int jobFormId)
        {
            var records = await _context.JobFormCVs
                .Where(cv => cv.isTelephoneInterviewPassed && cv.JobFormId == jobFormId)
                .Select(cv => new
                {
                    cv.Id,
                    cv.JobFormId,
                    cv.UserId,
                    User = _context.Users
                        .Where(u => u.Id == cv.UserId)
                        .Select(u => new
                        {
                            u.FullName,
                            u.Email,
                            u.PhoneNumber
                        }).FirstOrDefault(),
                    JobForm = _context.JobForms
                        .Where(jf => jf.Id == cv.JobFormId)
                        .Select(jf => jf.JobTitle) // Assuming the job form has a Title property for the job title
                        .FirstOrDefault()
                }).ToListAsync();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Records");

                // Add headers
                worksheet.Cells[1, 1].Value = "Full Name";
                worksheet.Cells[1, 2].Value = "Email";
                worksheet.Cells[1, 3].Value = "Phone Number";
                worksheet.Cells[1, 4].Value = "Job Title";

                // Add records
                for (int i = 0; i < records.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = records[i].User?.FullName;
                    worksheet.Cells[i + 2, 2].Value = records[i].User?.Email;
                    worksheet.Cells[i + 2, 3].Value = records[i].User?.PhoneNumber;
                    worksheet.Cells[i + 2, 4].Value = records[i].JobForm;
                }

                // Auto-fit columns for all cells
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                string excelName = $"TelephoneInterviewPassed_{jobFormId}_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.xlsx";

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        [HttpPut("{jobFormId}/{jobFormCVId}/toggle-technical-interview")]
        public async Task<IActionResult> ToggleTechnicalInterview(int jobFormId, int jobFormCVId, [FromBody] ToggleTechnicalInterviewRequest request)
        {
            try
            {
                var jobFormCV = await _context.JobFormCVs
                    .Where(cv => cv.JobFormId == jobFormId && cv.Id == jobFormCVId)
                    .FirstOrDefaultAsync();

                if (jobFormCV == null)
                {
                    return NotFound(new ServiceResult { Success = false, Message = "Job Form CV not found." });
                }

                jobFormCV.IsTechnicalInterviewPassed = request.Passed;

                try
                {
                    await _context.SaveChangesAsync();

                    // Send customized notification to the user
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == jobFormCV.UserId);
                    if (user == null)
                    {
                        return NotFound(new ServiceResult { Success = false, Message = "User not found." });
                    }

                    var hrMessage = request.HrMessage ?? "We look forward to the next steps.";

                    var notification = new Notification
                    {
                        UserId = jobFormCV.UserId,
                        Message = request.Passed ? $"Congratulations! {hrMessage}" : $"We're sorry to inform you that you did not pass the technical interview. {hrMessage}",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Notifications.Add(notification);
                    await _context.SaveChangesAsync();

                    // Send SignalR notification
                    await _hubContext.Clients.User(user.Id)
                        .SendAsync("ReceiveNotification", notification.Message);

                    // Send email notification
                    await _emailService.SendEmailAsync(
                        to: user.Email,
                        subject: "Technical Interview Result",
                        htmlContent: $"<p>{notification.Message}</p>",
                        plainTextContent: notification.Message
                    );

                    return Ok(new ServiceResult { Success = true, Message = $"Technical interview status toggled for Job Form CV ID {jobFormCVId}." });
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An error occurred while saving changes: {ex.Message}");
                    return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("technical-interview-passed-for-jobform/{jobFormId}")]
        public async Task<IActionResult> GetRecordsWithTechnicalInterviewPassedForJobForm(int jobFormId)
        {
            try
            {
                var records = await _context.JobFormCVs
                    .Where(cv => cv.IsTechnicalInterviewPassed && cv.JobFormId == jobFormId)
                    .Select(cv => new
                    {
                        cv.Id,
                        cv.JobFormId,
                        cv.UserId,
                        UserEmail = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.Email).FirstOrDefault(),
                        UserFullName = _context.Users.Where(u => u.Id == cv.UserId).Select(u => u.FullName).FirstOrDefault(),
                        cv.FilePath
                    })
                    .ToListAsync();

                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("export-technical-interview-passed-for-jobform/{jobFormId}")]
        public async Task<IActionResult> ExportRecordsWithTechnicalInterviewPassedForJobFormToExcel(int jobFormId)
        {
            try
            {
                var records = await _context.JobFormCVs
                    .Where(cv => cv.IsTechnicalInterviewPassed && cv.JobFormId == jobFormId)
                    .Select(cv => new
                    {
                        cv.Id,
                        cv.JobFormId,
                        cv.UserId,
                        User = _context.Users
                            .Where(u => u.Id == cv.UserId)
                            .Select(u => new
                            {
                                u.FullName,
                                u.Email,
                                u.PhoneNumber
                            }).FirstOrDefault(),
                        // Add other fields from JobFormCVs if needed
                    }).ToListAsync();

                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Records");

                    // Add headers
                    worksheet.Cells[1, 1].Value = "Full Name";
                    worksheet.Cells[1, 2].Value = "Email";
                    worksheet.Cells[1, 3].Value = "Phone Number";
                    // Add other headers if needed

                    // Add records
                    for (int i = 0; i < records.Count; i++)
                    {
                        worksheet.Cells[i + 2, 1].Value = records[i].User?.FullName;
                        worksheet.Cells[i + 2, 2].Value = records[i].User?.Email;
                        worksheet.Cells[i + 2, 3].Value = records[i].User?.PhoneNumber;
                        // Add other fields if needed
                    }

                    // Auto-fit columns for all cells
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    var stream = new MemoryStream();
                    package.SaveAs(stream);
                    stream.Position = 0;

                    string excelName = $"TechnicalInterviewPassed_JobForm_{jobFormId}_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
                    return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
    }
}
