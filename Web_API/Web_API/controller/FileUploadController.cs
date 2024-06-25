using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class FileUploadController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FileUploadController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("{postId}/upload-file")]
        public async Task<IActionResult> UploadFileForPost(int postId, IFormFile file)
        {
            try
            {
                int fileId = await _fileUploadService.UploadFileAsync(postId, file);
                return Ok(new { FileId = fileId });
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


        [HttpGet("{fileId}/url")]
        public async Task<IActionResult> GetFileUrl(int fileId)
        {
            try
            {
                var url = await _fileUploadService.GetFileUrlAsync(fileId);
                return Ok(url);
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
