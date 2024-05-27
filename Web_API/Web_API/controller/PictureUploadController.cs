using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PictureUploadController : ControllerBase
    {
        private readonly IPictureUploadService _pictureUploadService;

        public PictureUploadController(IPictureUploadService pictureUploadService)
        {
            _pictureUploadService = pictureUploadService ?? throw new ArgumentNullException(nameof(pictureUploadService));
        }

        [HttpPost("{postId}/upload-picture")]
        public async Task<IActionResult> UploadPictureForPost(int postId, IFormFile picture)
        {
            if (picture == null || picture.Length == 0)
            {
                return BadRequest("Picture is required.");
            }

            try
            {
                int pictureId = await _pictureUploadService.UploadPictureAsync(postId, picture);
                return Ok(new { PictureId = pictureId });
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

        [HttpGet("{pictureId}/picture-path")]
        public async Task<IActionResult> GetPicturePath(int pictureId)
        {
            try
            {
                var path = await _pictureUploadService.GetPicturePathAsync(pictureId);
                return Ok(path);
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
