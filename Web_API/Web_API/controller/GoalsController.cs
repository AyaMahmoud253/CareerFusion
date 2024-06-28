// GoalsController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_API.Models;
using Microsoft.AspNetCore.Cors;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class GoalsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public GoalsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // GET: api/goals/hruser/{hrUserId}
        [HttpGet("hruser/{hrUserId}")]
        public async Task<ActionResult<IEnumerable<Goal>>> GetGoalsForHRUser(string hrUserId)
        {
            var goals = await _context.Goals
                .Where(g => g.HRUserId == hrUserId)
                .ToListAsync();

            if (goals == null || !goals.Any())
            {
                return NotFound(new { Message = $"No goals found for HR user with ID {hrUserId}." });
            }

            return Ok(goals);
        }

        // POST: api/goals/hruser/{hrUserId}
        [HttpPost("hruser/{hrUserId}")]
        public async Task<ActionResult<Goal>> CreateGoal(string hrUserId, [FromBody] GoalInputModel model)
        {
            try
            {
                var goal = new Goal
                {
                    HRUserId = hrUserId, // Use HRUserId from the URL parameter
                    Description = model.Description,
                    Score = model.Score,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Goals.Add(goal);
                await _context.SaveChangesAsync();

                // Return 200 OK with the newly created goal
                return Ok(goal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
        // PUT: api/goals/{goalId}
        [HttpPut("{goalId}")]
        public async Task<ActionResult<Goal>> UpdateGoal(int goalId, [FromBody] GoalInputModel model)
        {
            try
            {
                var goal = await _context.Goals.FindAsync(goalId);

                if (goal == null)
                {
                    return NotFound(new { Message = $"Goal with ID {goalId} not found." });
                }

                // Update only the provided properties from the model
                if (model.Description != null)
                {
                    goal.Description = model.Description;
                }
                if (model.Score != null)
                {
                    goal.Score = model.Score;
                }

                _context.Goals.Update(goal);
                await _context.SaveChangesAsync();

                return Ok(goal);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


        // DELETE: api/goals/{goalId}
        [HttpDelete("{goalId}")]
        public async Task<ActionResult<Goal>> DeleteGoal(int goalId)
        {
            try
            {
                var goal = await _context.Goals.FindAsync(goalId);

                if (goal == null)
                {
                    return NotFound(new { Message = $"Goal with ID {goalId} not found." });
                }

                _context.Goals.Remove(goal);
                await _context.SaveChangesAsync();

                return Ok(new { Message = $"Goal with ID {goalId} has been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


    }
}
