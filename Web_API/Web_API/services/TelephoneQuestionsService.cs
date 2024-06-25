using Web_API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace Web_API.services
{
    public class TelephoneQuestionsService : ITelephoneQuestionsService
    {
        private readonly ApplicationDBContext _context;
        private readonly ILogger<TelephoneQuestionsService> _logger;

        public TelephoneQuestionsService(ApplicationDBContext context, ILogger<TelephoneQuestionsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult> AddTelephoneInterviewQuestionsAsync(int postId, List<TelephonePostQuestionModel> questions)
        {
            try
            {
                var post = await _context.Posts.FindAsync(postId);
                if (post == null)
                {
                    return new ServiceResult { Success = false, Message = "Post not found." };
                }

                var interviewQuestions = questions.Select(q => new TelephoneInterviewQuestion
                {
                    PostId = postId,
                    Question = q.Question
                }).ToList();

                _context.TelephonePostQuestions.AddRange(interviewQuestions);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Questions added successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<List<TelephonePostQuestionModel>> GetTelephoneInterviewQuestionsByPostAsync(int postId)
        {
            var questions = await _context.TelephonePostQuestions
                .Where(q => q.PostId== postId)
                .Select(q => new TelephonePostQuestionModel
                {
                    QuestionId = q.QuestionId,
                    Question = q.Question
                }).ToListAsync();

            return questions;
        }

        public async Task<ServiceResult> UpdateTelephoneInterviewQuestionAsync(int questionId, int postId, TelephonePostQuestionModel updatedQuestion)
        {
            try
            {
                var question = await _context.TelephonePostQuestions
                    .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.PostId == postId);

                if (question == null)
                {
                    return new ServiceResult { Success = false, Message = "Question not found." };
                }

                question.Question = updatedQuestion.Question;
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Question updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> DeleteTelephoneInterviewQuestionAsync(int questionId, int postId)
        {
            try
            {
                var question = await _context.TelephonePostQuestions
                    .FirstOrDefaultAsync(q => q.QuestionId == questionId && q.PostId == postId);

                if (question == null)
                {
                    return new ServiceResult { Success = false, Message = "Question not found." };
                }

                _context.TelephonePostQuestions.Remove(question);
                await _context.SaveChangesAsync();

                return new ServiceResult { Success = true, Message = "Question deleted successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
    }

}
