using Microsoft.AspNetCore.Mvc;
using Web_API.Models;
using Web_API.services;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Web_API.Services;

[Route("api/[controller]")]
[ApiController]
public class HiringTimelineController : ControllerBase
{
    private readonly IHiringTimelineService _hiringTimelineService;

    private readonly IJobFormService _jobFormService;


    public HiringTimelineController(IHiringTimelineService hiringTimelineService, IJobFormService jobFormService)
    {
        _hiringTimelineService = hiringTimelineService;
        _jobFormService = jobFormService;
    }

    [HttpPost("SetTimeline/{userId}")]
    public async Task<IActionResult> SetTimeline([FromRoute] string userId, [FromBody] SetTimelineModel model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        model.UserId = userId;
        var result = await _hiringTimelineService.SetHiringTimelineAsync(userId, model);

        if (!result.Success)
            return BadRequest(result.Message);

        return Ok(result.Message);
    }

    [HttpGet("GetTimelinesForUser/{userId}")]
    public async Task<IActionResult> GetTimelinesForUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("User ID is required.");

        var timelines = await _hiringTimelineService.GetTimelinesForUserAsync(userId);

        if (timelines == null || !timelines.Any())
            return NotFound($"No timelines found for user with ID {userId}.");

        // Assuming `timelines` is a collection of `TimelineStageModel` or similar
        return Ok(timelines);
    }

    [HttpPut("UpdateTimelineStage/{userId}/{stageId}")]
    public async Task<IActionResult> UpdateTimelineStage(string userId, int stageId, [FromBody] TimelineStageModel updatedStage)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _hiringTimelineService.UpdateTimelineStageAsync(userId, stageId, updatedStage);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Message);
    }
    [HttpDelete("DeleteTimelineStage/{userId}/{stageId}")]
    public async Task<IActionResult> DeleteTimelineStage(string userId, int stageId)
    {
        // Assuming you want to authorize the operation and ensure the user is deleting their own timeline stage
        // You may get the userId from the User claims if using JWT or other authentication mechanisms
        // var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest("User ID is required.");
        }

        var result = await _hiringTimelineService.DeleteTimelineStageAsync(userId, stageId);

        if (!result.Success)
        {
            return BadRequest(result.Message);
        }

        return Ok(result.Message);
    }
  

}

