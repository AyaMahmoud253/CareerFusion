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
    public class EvaluationsController : ControllerBase
    {
        private readonly ApplicationDBContext _context;

        public EvaluationsController(ApplicationDBContext context)
        {
            _context = context;
        }

        // POST: api/evaluations/{hrId}/questions
        [HttpPost("{hrId}/questions")]
        public async Task<ActionResult> CreateQuestionForHR(string hrId, [FromBody] CreateQuestionModel model)
        {
            try
            {
                var question = new EvaluationQuestion
                {
                    HRId = hrId,
                    Question = model.Question,
                    DefaultScore = model.DefaultScore
                };

                _context.EvaluationQuestions.Add(question);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Question added successfully.", QuestionId = question.Id });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        // GET: api/evaluations/{hrId}/questions
        [HttpGet("{hrId}/questions")]
        public async Task<ActionResult<IEnumerable<EvaluationQuestion>>> GetQuestionsByHRId(string hrId)
        {
            try
            {
                var questions = await _context.EvaluationQuestions
                    .Where(eq => eq.HRId == hrId)
                    .ToListAsync();

                if (questions == null || !questions.Any())
                {
                    return NotFound(new { Message = $"No evaluation questions found for HR ID {hrId}." });
                }

                return Ok(questions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }

        // POST: api/evaluations/{hrId}/questions/scores
        [HttpPost("{hrId}/questions/scores")]
        public async Task<ActionResult> AddScoresForUsers(string hrId, [FromBody] List<UserScoreModel> userScores)
        {
            try
            {
                var questions = await _context.EvaluationQuestions
                    .Where(eq => eq.HRId == hrId)
                    .ToListAsync();

                if (!questions.Any())
                {
                    return NotFound(new { Message = $"No questions found for HR ID {hrId}." });
                }

                var userQuestionScores = userScores.Select(userScore => new UserQuestionScore
                {
                    UserId = userScore.UserId,
                    EvaluationQuestionId = userScore.QuestionId,
                    Score = userScore.Score
                }).ToList();

                _context.UserQuestionScores.AddRange(userQuestionScores);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Scores assigned successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }
        // GET: api/evaluations/{userId}/{hrId}/overallscore/{expectedScore}
        [HttpGet("{userId}/{hrId}/overallscore/{expectedScore}")]
        public async Task<ActionResult<UserEvaluation>> GetUserOverallScore(string userId, string hrId, double expectedScore)
        {
            try
            {
                var userScores = await _context.UserQuestionScores
                    .Include(uqs => uqs.EvaluationQuestion)
                    .Where(uqs => uqs.UserId == userId && uqs.EvaluationQuestion.HRId == hrId)
                    .ToListAsync();

                if (userScores == null || !userScores.Any())
                {
                    return NotFound(new { Message = $"No evaluation scores found for user with ID {userId} for HR ID {hrId}." });
                }

                var overallScore = userScores.Average(uqs => uqs.Score);

                // Prepare the user evaluation result
                var userEvaluation = new UserEvaluation
                {
                    UserId = userId,
                    OverallScore = overallScore,
                    Questions = userScores.Select(uqs => uqs.EvaluationQuestion).Distinct().ToList()
                };

                // Determine the comparison result
                var comparisonResult = overallScore >= expectedScore ? "Met or Exceeded" : "Below Expected";

                // Create and save the notification
                var notificationMessage = $"Your overall score of {overallScore} has {comparisonResult} the expected score of {expectedScore}.";
                var notification = new Notification
                {
                    UserId = userId,
                    Message = notificationMessage,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Notifications.Add(notification);
                await _context.SaveChangesAsync();

                // Return the evaluation result and status
                return Ok(new
                {
                    UserEvaluation = userEvaluation,
                    Status = comparisonResult
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = $"An error occurred: {ex.Message}" });
            }
        }


    }
}
