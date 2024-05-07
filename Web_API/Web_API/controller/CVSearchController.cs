using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CVSearchController : ControllerBase
    {
        private readonly IJobFormCVService _jobFormCVService;

        public CVSearchController(IJobFormCVService jobFormCVService)
        {
            _jobFormCVService = jobFormCVService;
        }

        // Action for retrieving all CVs based on job title
        [HttpGet("cvs-by-title/{jobTitle}")]
        public async Task<ActionResult<IEnumerable<string>>> GetCVsByTitle(string jobTitle)
        {
            var cvFilePaths = await _jobFormCVService.GetCVFilePathsByTitleAsync(jobTitle);

            if (cvFilePaths == null || !cvFilePaths.Any())
            {
                return NotFound($"No CVs found for job title: {jobTitle}");
            }

            return Ok(cvFilePaths);
        }
    }
}
