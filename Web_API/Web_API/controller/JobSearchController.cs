// JobSearchController.cs
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Web_API.Models;
using Web_API.Services;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JobSearchController : ControllerBase
    {
        private readonly IJobSearchService _jobSearchService;

        public JobSearchController(IJobSearchService jobSearchService)
        {
            _jobSearchService = jobSearchService;
        }

        [HttpGet("SearchByJobTitle")]
        public async Task<ActionResult<IEnumerable<JobFormEntity>>> SearchByJobTitle(string keyword)
        {
            var openPositions = await _jobSearchService.SearchOpenPositions(keyword);
            return Ok(openPositions);
        }

        [HttpGet("SearchTitleWithLocation")]
        public async Task<ActionResult<IEnumerable<JobFormEntity>>> SearchTitleWithLocation(string keyword, string location)
        {
            var openPositions = await _jobSearchService.SearchOpenPositionsWithLocation(keyword, location);
            return Ok(openPositions);
        }
    }
}
