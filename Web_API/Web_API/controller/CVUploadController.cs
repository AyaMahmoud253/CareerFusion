using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVUploadController : ControllerBase
    {
        private readonly ICVUploadService _cvUploadService;

        public CVUploadController(ICVUploadService cvUploadService)
        {
            _cvUploadService = cvUploadService ?? throw new ArgumentNullException(nameof(cvUploadService));
        }

        [HttpPost("{postId}/upload-postcv")]
        public async Task<IActionResult> UploadCVForPost(int postId, IFormFile cvFile)
        {
            try
            {
                var result = await _cvUploadService.UploadFileAsync(postId, cvFile);
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
    }
}
