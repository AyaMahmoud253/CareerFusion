using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
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

        public OpenPosCVController(ApplicationDBContext context, IWebHostEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // Action for uploading CV for a specific job form
        [HttpPost("{jobFormId}/upload-cv")]
        public async Task<IActionResult> UploadCVForJobForm(int jobFormId, IFormFile cvFile)
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
                    FilePath = urlPath // Store URL path instead of file path
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

        // Action for retrieving all CVs associated with a specific job form
        [HttpGet("{jobFormId}/cvs")]
        public async Task<ActionResult<IEnumerable<string>>> GetCVsForJobForm(int jobFormId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobFormCVs)
                .FirstOrDefaultAsync(jf => jf.Id == jobFormId);

            if (jobForm == null)
            {
                return NotFound($"Job form with ID {jobFormId} not found.");
            }

            // Convert file paths to URLs
            var cvFilePaths = jobForm.JobFormCVs.Select(cv => cv.FilePath).ToList();
            return Ok(cvFilePaths);
        }

        // Other actions...
    }
}
