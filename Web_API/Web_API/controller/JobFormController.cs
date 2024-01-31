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


    }
}
