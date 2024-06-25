using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_API.Services;
using System.IO.Compression;
using Web_API.services;
using Web_API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Cors;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class CVUploadController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICVUploadService _cvUploadService;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<CVUploadController> _logger;


        public CVUploadController(ApplicationDBContext context, UserManager<ApplicationUser> userManager, ICVUploadService cvUploadService, IWebHostEnvironment hostingEnvironment, ILogger<CVUploadController> logger)
        {
            _context = context;
            _userManager = userManager;
            _cvUploadService = cvUploadService ?? throw new ArgumentNullException(nameof(cvUploadService));
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;

        }

        [HttpPost("{postId}/upload-postcv")]
        public async Task<IActionResult> UploadCVForPost(int postId, string userId, IFormFile cvFile)
        {
            if (cvFile == null || cvFile.Length == 0)
            {
                return BadRequest("CV file is required.");
            }

            var post = await _context.Posts.FindAsync(postId);
            if (post == null)
            {
                return NotFound($"Post with ID {postId} not found.");
            }

            try
            {
                string result = await _cvUploadService.UploadFileAsync(postId, userId, cvFile);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }



        [HttpGet("{postId}/cv-paths")]
        public async Task<IActionResult> GetCVPathsForPost(int postId)
        {
            try
            {
                var cvPaths = await _cvUploadService.GetCVPathsForPostAsync(postId);
                return Ok(cvPaths);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // New endpoint for downloading all CVs as a ZIP file
        [HttpGet("{postId}/download-cvs")]
        public async Task<IActionResult> DownloadCVsForPost(int postId)
        {
            try
            {
                // Fetch CV paths for the given postId
                var cvPaths = await _cvUploadService.GetCVPathsForPostAsync(postId);

                // Check if cvPaths is null or empty
                if (cvPaths == null)
                {
                    return NotFound($"No CVs found for post ID {postId}.");
                }

                // Create a temporary directory for storing the files to be zipped
                var tempDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "temp");
                Directory.CreateDirectory(tempDirectory);

                try
                {
                    // Copy each file to the temp directory
                    foreach (var cvPath in cvPaths)
                    {
                        var sourceFilePath = GetPhysicalFilePath(cvPath);
                        var fileName = Path.GetFileName(cvPath);
                        var destFilePath = Path.Combine(tempDirectory, fileName);

                        if (!System.IO.File.Exists(sourceFilePath))
                        {
                            return NotFound($"File not found: {cvPath}");
                        }

                        System.IO.File.Copy(sourceFilePath, destFilePath, true);
                    }

                    // Prepare a unique file name for the zip file
                    string zipFileName = $"CVs_Post_{postId}_{DateTime.Now:yyyyMMddHHmmss}.zip";
                    string zipFilePath = Path.Combine(_hostingEnvironment.WebRootPath, zipFileName);

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
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        // get the physical file path from URL
        private string GetPhysicalFilePath(string urlPath)
        {
            var uri = new Uri(urlPath);
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, uri.AbsolutePath.TrimStart('/').Replace("/", "\\"));
            return filePath;
        }

        [HttpPut("update-screened/{postId}")]
        public async Task<IActionResult> UpdateScores(int postId, [FromBody] List<string> emails)
        {
            try
            {
                var success = await _cvUploadService.UpdateScreeningStatusAsync(postId, emails);

                if (success)
                {
                    return Ok(new ServiceResult { Success = true, Message = "Screening status updated successfully." });
                }
                else
                {
                    return StatusCode(500, new ServiceResult { Success = false, Message = "Failed to update screening status." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("screened/{postId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetScreenedUsers(int postId)
        {
            try
            {
                var records = await _cvUploadService.GetScreenedUsersAsync(postId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{postId}/set-telephone-interview-date/{id}")]
        public async Task<IActionResult> SetTelephoneInterviewDate(int id, int postId,  DateTime interviewDate)
        {
            try
            {
                var result = await _cvUploadService.SetTelephoneInterviewDateForPostAsync(id, postId, interviewDate);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{postId}/get-telephone-interview-date/{id}")]
        public async Task<IActionResult> GetTelephoneInterviewDate(int id, int postId)
        {
            try
            {
                var result = await _cvUploadService.GetTelephoneInterviewDateForPostAsync(id, postId);
                if (result.Success)
                {
                    return Ok(result.Data);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpPut("{postId}/{postCVId}/toggle-telephone-interview")]
        public async Task<IActionResult> ToggleTelephoneInterview(int postId, int postCVId)
        {
            try
            {
                var result = await _cvUploadService.ToggleTelephoneInterviewForPostCVAsync(postId, postCVId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("telephone-interview-passed/{postId}")]
        public async Task<IActionResult> GetRecordsWithTelephoneInterviewPassed(int postId)
        {
            var records = await _cvUploadService.GetRecordsWithTelephoneInterviewPassedAsync(postId);
            return Ok(records);
        }

        [HttpGet("export-telephone-interview-passed/{postId}")]
        public async Task<IActionResult> ExportRecordsWithTelephoneInterviewPassedToExcel(int postId)
        {
            try
            {
                var stream = await _cvUploadService.ExportRecordsWithTelephoneInterviewPassedToExcelAsync(postId);
                string excelName = $"TelephoneInterviewPassed_{postId}_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{postCVId}/post/{postId}/set-technical-interview-date")]
        public async Task<IActionResult> SetTechnicalInterviewDate(int postCVId, int postId, DateTime? technicalAssessmentDate, DateTime? physicalInterviewDate)
        {
            try
            {
                var result = await _cvUploadService.SetTechnicalInterviewDateAsync(postCVId, postId, technicalAssessmentDate, physicalInterviewDate);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


        [HttpGet("{postCVId}/post/{postId}/get-technical-assessment-date")]
        public async Task<IActionResult> GetTechnicalAssessmentDate(int postCVId, int postId)
        {
            try
            {
                var result = await _cvUploadService.GetTechnicalAssessmentDateAsync(postCVId, postId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("{postCVId}/post/{postId}/get-physical-interview-date")]
        public async Task<IActionResult> GetPhysicalInterviewDate(int postCVId, int postId)
        {
            try
            {
                var result = await _cvUploadService.GetPhysicalInterviewDateAsync(postCVId, postId);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpPut("{postId}/{postCVId}/toggle-technical-interview")]
        public async Task<IActionResult> ToggleTechnicalInterview(int postId, int postCVId, [FromBody] ToggleTechnicalInterviewRequest request)
        {
            try
            {
                var result = await _cvUploadService.ToggleTechnicalInterviewForPostCVAsync(postId, postCVId, request.Passed, request.HrMessage);
                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        [HttpGet("technical-interview-passed/{postId}")]
        public async Task<IActionResult> GetRecordsWithTechnicalInterviewPassed(int postId)
        {
            var records = await _cvUploadService.GetRecordsWithTechnicalInterviewPassedAsync(postId);
            return Ok(records);
        }

        [HttpGet("export-technical-interview-passed/{postId}")]
        public async Task<IActionResult> ExportRecordsWithTechnicalInterviewPassedToExcel(int postId)
        {
            try
            {
                var stream = await _cvUploadService.ExportRecordsWithTechnicalInterviewPassedToExcelAsync(postId);
                string excelName = $"TechnicalInterviewPassed_{postId}_{DateTime.Now:yyyyMMddHHmmssfff}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
            catch (Exception ex)
            {
                // Log the error
                return StatusCode(500, new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }





    }
}
