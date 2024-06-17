using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OpenPosCVController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpenPosCVController(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userManager = userManager;
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

            string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "cv-uploads");
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            try
            {
                string uniqueFileName = $"{Guid.NewGuid().ToString()}_{Path.GetFileName(cvFile.FileName)}";
                string filePath = Path.Combine(uploadDirectory, uniqueFileName);

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

                return Ok("CV uploaded successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred while uploading the CV: {ex.Message}");
            }
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
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.FullName,
                    u.PhoneNumber,
                    u.Email
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            return Ok(user);
        }

        // Other actions...
    }
}
