using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System;
using System.IO;
using System.Threading.Tasks;



namespace Web_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDBContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserProfileController(UserManager<ApplicationUser> userManager, ApplicationDBContext context, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateProfile(string userId, [FromBody] UserProfileUpdateDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            // Update the properties if they are provided
            if (model.Title != null)
            {
                user.Title = model.Title;
            }
            if (model.ProfilePicturePath != null)
            {
                user.ProfilePicturePath = model.ProfilePicturePath;
            }
            if (model.FollowersCount.HasValue)
            {
                user.FollowersCount = model.FollowersCount.Value;
            }

            // Update the user first, to ensure it doesn't fail after modifying courses and project links
            var userUpdateResult = await _userManager.UpdateAsync(user);
            if (!userUpdateResult.Succeeded)
            {
                return BadRequest(userUpdateResult.Errors);
            }

            // Handle Courses
            if (model.Courses != null)
            {
                foreach (var courseDto in model.Courses)
                {
                    var course = await _context.Courses
                        .FirstOrDefaultAsync(c => c.CourseId == courseDto.CourseId && c.UserId == userId);

                    if (course != null)
                    {
                        course.CourseName = courseDto.CourseName;
                    }
                    else
                    {
                        course = new Course
                        {
                            CourseName = courseDto.CourseName,
                            UserId = userId
                        };
                        _context.Courses.Add(course);
                    }
                }
            }

            // Handle ProjectLinks
            if (model.ProjectLinks != null)
            {
                foreach (var projectDto in model.ProjectLinks)
                {
                    ProjectLink projectLink;
                    if (projectDto.ProjectLinkId.HasValue) // Existing project link
                    {
                        projectLink = await _context.ProjectLinks.FirstOrDefaultAsync(p => p.ProjectLinkId == projectDto.ProjectLinkId.Value && p.UserId == userId);
                        if (projectLink != null)
                        {
                            projectLink.ProjectName = projectDto.ProjectName;
                            projectLink.ProjectUrl = projectDto.ProjectUrl;
                        }
                    }
                    else // New project link
                    {
                        projectLink = new ProjectLink
                        {
                            ProjectName = projectDto.ProjectName,
                            ProjectUrl = projectDto.ProjectUrl,
                            UserId = userId
                        };
                        _context.ProjectLinks.Add(projectLink);
                    }
                }
            }
            // Handle Skills
            if (model.Skills != null)
            {
                foreach (var skillDto in model.Skills)
                {
                    Skill skill;
                    if (skillDto.SkillId.HasValue && skillDto.SkillId.Value != 0) // Existing skill
                    {
                        skill = await _context.Skills.FirstOrDefaultAsync(s => s.SkillId == skillDto.SkillId.Value && s.UserId == userId);
                        if (skill != null)
                        {
                            skill.SkillName = skillDto.SkillName;
                            skill.SkillLevel = skillDto.SkillLevel; // Update this as needed
                        }
                    }
                    else // New skill
                    {
                        skill = new Skill
                        {
                            SkillName = skillDto.SkillName,
                            SkillLevel = skillDto.SkillLevel, // Set this as needed
                            UserId = userId
                        };
                        _context.Skills.Add(skill);
                    }
                }
            }

            // Save the changes to the user, courses, and project links
            await _context.SaveChangesAsync();

            return Ok("Profile updated successfully.");
        }
        [HttpPost("upload-profile-picture/{userId}")]
        public async Task<IActionResult> UploadProfilePicture(string userId, IFormFile profilePicture)
        {
            if (profilePicture != null && profilePicture.Length > 0)
            {
                try
                {
                    // Find the user with the specified userId
                    var user = await _userManager.FindByIdAsync(userId);

                    if (user != null)
                    {
                        // Generate a unique filename for the uploaded image (e.g., using Guid)
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(profilePicture.FileName);

                        // Specify the directory where you want to save the image (e.g., within the web root)
                        string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "profile-pictures");

                        // Ensure the directory exists; create it if it doesn't
                        Directory.CreateDirectory(uploadDirectory);

                        // Combine the directory and filename to get the full path
                        string filePath = Path.Combine(uploadDirectory, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await profilePicture.CopyToAsync(stream);
                        }

                        // Store the relative path or URL to the image in your database
                        string relativePath = "/profile-pictures/";
                        string imagePath = relativePath + uniqueFileName;

                        // Update the user's profile picture path in your database
                        user.ProfilePicturePath = imagePath;
                        await _userManager.UpdateAsync(user);

                        return Ok(new { imagePath });
                    }
                    else
                    {
                        return NotFound($"User with ID {userId} not found.");
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

            // Handle error case if no file was uploaded
            return BadRequest("No file uploaded");
        }




        [HttpGet("{userId}")]
        public async Task<IActionResult> GetProfile(string userId)
        {
            var user = await _userManager.Users
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.UserName,
                    u.FullName,
                    u.Title,
                    u.ProfilePicturePath,
                    u.FollowersCount,
                    Courses = u.Courses.Select(c => new
                    {
                        c.CourseId,
                        c.CourseName
                        // Include other properties you want to return
                    }).ToList(),
                    ProjectLinks = u.ProjectLinks.Select(p => new
                    {
                        p.ProjectLinkId,
                        p.ProjectName,
                        p.ProjectUrl
                        // Include other properties you want to return
                    }).ToList(),
                    Skills = u.Skills.Select(s => new
                    {
                        s.SkillId,
                        s.SkillName,
                        s.SkillLevel
                        // Include other properties you want to return
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            return Ok(user);
        }
        [HttpGet("{userId}/profile-picture")]
        public async Task<IActionResult> GetProfilePicture(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            if (string.IsNullOrEmpty(user.ProfilePicturePath))
            {
                return NotFound("Profile picture not set for this user.");
            }

            var profilePictureUrl = CreateFullProfilePictureUrl(user.ProfilePicturePath);
            return Ok(new { profilePictureUrl });
        }

        // ... existing actions ...

        private string CreateFullProfilePictureUrl(string profilePicturePath)
        {
            // Assuming that the profile picture path is a relative path stored in the database
            var request = this.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}{request.PathBase}{profilePicturePath}";
        }

    }
}
