using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Web_API.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForRecommendController : ControllerBase
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ForRecommendController(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new {
                    u.Id,
                    u.Title,
                    u.Address,
                    CombinedSkills = string.Join(", ", u.Skills.Select(s => s.SkillName))
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs()
        {
            var jobs = await _context.JobForms
                .Select(j => new {
                    j.Id,
                    j.JobTitle,
                    j.JobType,
                    j.JobLocation,
                    Skills = j.JobSkills.Select(js => js.SkillName).ToList(),
                    Descriptions = j.JobDescriptions.Select(jd => jd.Description).ToList()
                })
                .ToListAsync();

            return Ok(jobs);
        }

        // Add other endpoints for Skills, JobSkills, and JobDescriptions as needed
    }
}
