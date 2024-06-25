using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web_API.Models;
using Web_API.services;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")]
    public class JobFormController : ControllerBase
    {
        private readonly IJobFormService _jobFormService;
        private readonly UserManager<ApplicationUser> _userManager;


        public JobFormController(IJobFormService jobFormService, UserManager<ApplicationUser> userManager)
        {
            _jobFormService = jobFormService;
            _userManager = userManager;
        }
    
        [HttpPost("add/{userId}")]
        public async Task<IActionResult> AddJobFormAsync([FromRoute] string userId, [FromBody] JobFormModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            model.UserId = userId; // Assign userId to the model

            var result = await _jobFormService.AddJobFormAsync(userId, model);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        [HttpGet("OpenPos/{userId}")]
        public async Task<IActionResult> GetJobsByUserAsync([FromRoute] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                return BadRequest("User ID must be provided.");
            }

            var jobs = await _jobFormService.GetJobsByUserIdAsync(userId);

            if (jobs != null)
            {
                return Ok(jobs);
            }

            return NotFound($"No jobs found for user with ID: {userId}");
        }
        [HttpGet("jobDetails/{userId}/{jobId}")]
        public async Task<IActionResult> GetSpecificUserJob([FromRoute] string userId, [FromRoute] int jobId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID must be provided.");
            }

            var jobForm = await _jobFormService.GetSpecificJobForUserAsync(userId, jobId);
            if (jobForm != null)
            {
                return Ok(jobForm);
            }
            else
            {
                return NotFound($"No job found with ID {jobId} for user {userId}.");
            }
        }
        [HttpPut("updateResponsibility/{userId}/{jobId}/{responsibilityId}")]
        public async Task<IActionResult> UpdateJobResponsibility([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int responsibilityId, [FromBody] JobResponsibilityModel responsibilityModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _jobFormService.UpdateJobResponsibilityAsync(userId, jobId, responsibilityId, responsibilityModel);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpPut("updateSkill/{userId}/{jobId}/{skillId}")]
        public async Task<IActionResult> UpdateJobSkill([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int skillId, [FromBody] JobSkillModel skillModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _jobFormService.UpdateJobSkillAsync(userId, jobId, skillId, skillModel);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpPut("updateDescription/{userId}/{jobId}/{descriptionId}")]
        public async Task<IActionResult> UpdateJobDescription([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int descriptionId, [FromBody] JobDescriptionModel descriptionModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _jobFormService.UpdateJobDescriptionAsync(userId, jobId, descriptionId, descriptionModel);
            if (result.Success)
            {
                return Ok(result.Message);
            }
            else
            {
                return BadRequest(result.Message);
            }
        }
        [HttpDelete("DeleteJobSkill/{userId}/{jobId}/{skillId}")]
        public async Task<IActionResult> DeleteJobSkillAsync([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int skillId)
        {
            var result = await _jobFormService.DeleteJobSkillAsync(userId, jobId, skillId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpDelete("DeleteJobDescription/{userId}/{jobId}/{descriptionId}")]
        public async Task<IActionResult> DeleteJobDescriptionAsync([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int descriptionId)
        {
            var result = await _jobFormService.DeleteJobDescriptionAsync(userId, jobId, descriptionId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        [HttpDelete("DeleteJobResponsibility/{userId}/{jobId}/{responsibilityId}")]
        public async Task<IActionResult> DeleteJobResponsibilityAsync([FromRoute] string userId, [FromRoute] int jobId, [FromRoute] int responsibilityId)
        {
            var result = await _jobFormService.DeleteJobResponsibilityAsync(userId, jobId, responsibilityId);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }
        // New endpoint to add telephone interview questions to an existing job form
        [HttpPost("add-telephone-interview-questions/{userId}/{jobId}")]
        public async Task<IActionResult> AddTelephoneInterviewQuestions([FromRoute] string userId, [FromRoute] int jobId, [FromBody] List<TelephoneInterviewQuestionModel> questions)
        {
            var result = await _jobFormService.AddTelephoneInterviewQuestionsAsync(userId, jobId, questions);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
        [HttpGet("getTelephoneInterviewQuestionsByJobTitle/{jobTitle}")]
        public async Task<IActionResult> GetTelephoneInterviewQuestionsByJobTitle(string jobTitle)
        {
            var result = await _jobFormService.GetTelephoneInterviewQuestionsByJobTitleAsync(jobTitle);
            return Ok(result);
        }

        [HttpPut("update-telephone-interview-question")]
        public async Task<IActionResult> UpdateTelephoneInterviewQuestion(int questionId, string jobTitle, TelephoneInterviewQuestionModel question)
        {
            var result = await _jobFormService.UpdateTelephoneInterviewQuestionAsync(questionId, jobTitle, question);
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result);
        }


        [HttpDelete("deletetelephoneinterviewquestion/{questionId}/{jobTitle}")]
        public async Task<IActionResult> DeleteTelephoneInterviewQuestionAsync(int questionId, string jobTitle)
        {
            var result = await _jobFormService.DeleteTelephoneInterviewQuestionAsync(questionId, jobTitle);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }

        [HttpGet("all-open-positions")]
        public async Task<IActionResult> GetAllOpenPositions()
        {
            var result = await _jobFormService.GetAllOpenPositionsAsync();

            if (result != null && result.Any())
            {
                return Ok(result);
            }

            return NotFound("No open positions found.");
        }





    }
}
