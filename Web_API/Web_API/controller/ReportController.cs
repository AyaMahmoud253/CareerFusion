using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Web_API.Models;
using Web_API.services;

namespace Web_API.controller
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // POST: api/Report/Create/{hrUserId}
        [HttpPost("Create/{hrUserId}")]
        public async Task<IActionResult> CreateReport(string hrUserId, [FromBody] ReportCreateModelDTO model)
        {
            if (model == null)
            {
                return BadRequest("Report model is null.");
            }

            try
            {
                var reportId = await _reportService.CreateReportAsync(model, hrUserId);
                return Ok(new { ReportId = reportId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // POST: api/Report/{reportId}/SendTo/{userId}
        [HttpPost("{reportId}/SendTo/{userId}")]
        public async Task<IActionResult> SendReportToRecipient(int reportId, string userId)
        {
            try
            {
                await _reportService.SendReportToRecipientAsync(reportId, userId);
                return Ok("Report sent to recipient successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Report/User/{userId}
        [HttpGet("User/{userId}")]
        public async Task<IActionResult> GetReportsForUser(string userId)
        {
            try
            {
                var reports = await _reportService.GetReportsForUserAsync(userId);
                return Ok(reports);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Report/{reportId}/Accept
        [HttpPut("{reportId}/Accept")]
        public async Task<IActionResult> ToggleReportAcceptance(int reportId, [FromQuery] string userId, [FromQuery] bool accept)
        {
            try
            {
                var result = await _reportService.ToggleReportAcceptanceAsync(reportId, userId, accept);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Report/{reportId}/Read
        [HttpPut("{reportId}/Read")]
        public async Task<IActionResult> ToggleReportRead(int reportId, [FromQuery] string userId, [FromQuery] bool isRead)
        {
            try
            {
                var result = await _reportService.ToggleReportReadAsync(reportId, userId, isRead);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
