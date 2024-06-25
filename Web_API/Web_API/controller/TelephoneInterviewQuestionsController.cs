using Web_API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.services;
using Microsoft.AspNetCore.Cors;

namespace Web_API.controller
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class TelephoneInterviewQuestionsController : ControllerBase
    {
        private readonly ITelephoneQuestionsService _telephoneInterviewQuestionsService;
        private readonly ILogger<TelephoneInterviewQuestionsController> _logger;

        public TelephoneInterviewQuestionsController(ITelephoneQuestionsService telephoneInterviewQuestionsService, ILogger<TelephoneInterviewQuestionsController> logger)
        {
            _telephoneInterviewQuestionsService = telephoneInterviewQuestionsService;
            _logger = logger;
        }

        [HttpPost("add-telephone-interview-questions/{postId}")]
        public async Task<IActionResult> AddTelephoneInterviewQuestions([FromRoute] int postId, [FromBody] List<TelephonePostQuestionModel> questions)
        {
            var result = await _telephoneInterviewQuestionsService.AddTelephoneInterviewQuestionsAsync(postId, questions);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpGet("getTelephoneInterviewQuestionsByPost/{postId}")]
        public async Task<IActionResult> GetTelephoneInterviewQuestionsByPoast([FromRoute] int postId)
        {
            var result = await _telephoneInterviewQuestionsService.GetTelephoneInterviewQuestionsByPostAsync(postId);
            return Ok(result);
        }

        [HttpPut("update-telephone-interview-question/{postId}/{questionId}")]
        public async Task<IActionResult> UpdateTelephoneInterviewQuestion([FromRoute] int postId, [FromRoute] int questionId, [FromBody] TelephonePostQuestionModel question)
        {
            var result = await _telephoneInterviewQuestionsService.UpdateTelephoneInterviewQuestionAsync(questionId, postId, question);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }

        [HttpDelete("delete-telephone-interview-question/{postId}/{questionId}")]
        public async Task<IActionResult> DeleteTelephoneInterviewQuestionAsync([FromRoute] int postId, [FromRoute] int questionId)
        {
            var result = await _telephoneInterviewQuestionsService.DeleteTelephoneInterviewQuestionAsync(questionId, postId);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }
    }
}
