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
            if (model.Description != null)
            {
                user.Description = model.Description;
            }
            if (model.Address != null)
            {
                user.Address = model.Address;
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

            // Handle Courses
            if (model.ProjectLinks != null)
            {
                foreach (var projectDto in model.ProjectLinks)
                {
                    var project = await _context.ProjectLinks
                        .FirstOrDefaultAsync(p => p.ProjectLinkId == projectDto.ProjectLinkId && p.UserId == userId);

                    if (project != null)
                    {
                        project.ProjectName = projectDto.ProjectName;
                        project.ProjectUrl= projectDto.ProjectUrl;
                        
                    }
                    else
                    {
                        project = new ProjectLink
                        {
                            ProjectName = projectDto.ProjectName,
                            ProjectUrl = projectDto.ProjectUrl,
                            UserId = userId
                        };
                        _context.ProjectLinks.Add(project);
                    }
                }
            }

            



            // Handle skills
            if (model.Skills != null)
            {
                foreach (var skillDto in model.Skills)
                {
                    var skill = await _context.Skills
                        .FirstOrDefaultAsync(s => s.SkillId == skillDto.SkillId && s.UserId == userId);

                    if (skill != null)
                    {
                        skill.SkillName = skillDto.SkillName;
                        skill.SkillLevel = skillDto.SkillLevel;

                    }
                    else
                    {
                        skill = new Skill
                        {
                            SkillName = skillDto.SkillName,
                            SkillLevel = skillDto.SkillLevel,

                            UserId = userId
                        };
                        _context.Skills.Add(skill);
                    }
                }
            }

            // Handle SiteLinks
            if (model.SiteLinks != null)
            {
                foreach (var siteDto in model.SiteLinks)
                {
                    SiteLink siteLink;
                    if (siteDto.LinkId.HasValue) // Existing project link
                    {
                        siteLink = await _context.SiteLinks.FirstOrDefaultAsync(sl => sl.LinkId == siteDto.LinkId.Value && sl.UserId == userId);
                        if (siteLink != null)
                        {
                            siteLink.LinkUrl = siteDto.LinkUrl;
                        }
                    }
                    else // New site link
                    {
                        siteLink = new SiteLink
                        {

                            LinkUrl = siteDto.LinkUrl,
                            UserId = userId
                        };
                        _context.SiteLinks.Add(siteLink);
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
                    u.Description, // Add Description
                    u.Address,     // Add Address
                    ProjectLinks = u.ProjectLinks.Select(p => new
                    {
                        p.ProjectLinkId,
                        p.ProjectName,
                        p.ProjectUrl
                        // Include other properties you want to return
                    }).ToList(),
                    Courses = u.Courses.Select(c => new
                    {
                        c.CourseId,
                        c.CourseName
                        // Include other properties you want to return
                    }).ToList(),
                    
                    Skills = u.Skills.Select(s => new
                    {
                        s.SkillId,
                        s.SkillName,
                        s.SkillLevel
                        // Include other properties you want to return
                    }).ToList(),
                    SiteLinks = u.SiteLinks.Select(sl => new
                    {
                        sl.LinkId,
                        sl.LinkUrl
                        // Include other properties you want to return
                    }).ToList() // Add SiteLinks
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

        [HttpDelete("{userId}/projectlinks/{projectLinkId}")]
        public async Task<IActionResult> DeleteProjectLink(string userId, int projectLinkId)
        {
            // Find the project link by ID and ensure it belongs to the user
            var projectLink = await _context.ProjectLinks
                .FirstOrDefaultAsync(p => p.ProjectLinkId == projectLinkId && p.UserId == userId);

            if (projectLink == null)
            {
                return NotFound($"Project link with ID {projectLinkId} for user ID {userId} not found.");
            }

            // Delete the project link
            _context.ProjectLinks.Remove(projectLink);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok($"Project link with ID {projectLinkId} has been successfully deleted from user ID {userId}'s profile.");
        }

        [HttpDelete("{userId}/courses/{courseId}")]
        public async Task<IActionResult> DeleteCourse(string userId, int courseId)
        {
            // Find the course by ID and ensure it belongs to the user
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == courseId && c.UserId == userId);

            if (course == null)
            {
                return NotFound($"Course with ID {courseId} for user ID {userId} not found.");
            }

            // Delete the course
            _context.Courses.Remove(course);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok($"Course with ID {courseId} has been successfully deleted from user ID {userId}'s profile.");
        }

        [HttpDelete("{userId}/skills/{skillId}")]
        public async Task<IActionResult> DeleteSkill(string userId, int skillId)
        {
            // Find the skill by ID and ensure it belongs to the user
            var skill = await _context.Skills
                .FirstOrDefaultAsync(s => s.SkillId == skillId && s.UserId == userId);

            if (skill == null)
            {
                return NotFound($"Skill with ID {skillId} for user ID {userId} not found.");
            }

            // Delete the skill
            _context.Skills.Remove(skill);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok($"Skill with ID {skillId} has been successfully deleted from user ID {userId}'s profile.");
        }

        [HttpDelete("{userId}/sitelinks/{siteLinkId}")]
        public async Task<IActionResult> DeleteSiteLink(string userId, int siteLinkId)
        {
            // Find the site link by ID and ensure it belongs to the user
            var siteLink = await _context.SiteLinks
                .FirstOrDefaultAsync(sl => sl.LinkId == siteLinkId && sl.UserId == userId);

            if (siteLink == null)
            {
                return NotFound($"Site link with ID {siteLinkId} for user ID {userId} not found.");
            }

            // Delete the site link
            _context.SiteLinks.Remove(siteLink);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            return Ok($"Site link with ID {siteLinkId} has been successfully deleted from user ID {userId}'s profile.");
        }
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                try
                {
                    // Specify the directory within wwwroot where you want to save the file
                    string uploadDirectory = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

                    // Ensure the directory exists; create it if it doesn't
                    Directory.CreateDirectory(uploadDirectory);

                    // Combine the directory and filename to get the full path
                    string filePath = Path.Combine(uploadDirectory, "CV.docx");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    return Ok("File uploaded successfully.");
                }
                catch (Exception ex)
                {
                    return BadRequest($"Error: {ex.Message}");
                }
            }

            return BadRequest("No file uploaded");
        }
        [HttpGet("download-file")]
        public IActionResult DownloadFile()
        {
            // Define the path to the file within wwwroot
            var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", "CV.docx");

            // Check if the file exists
            if (System.IO.File.Exists(filePath))
            {
                // Determine the content type (MIME type)
                var contentType = "application/octet-stream"; // Adjust based on your file type

                // Return the file as a download
                return PhysicalFile(filePath, contentType, "CV.docx");
            }

            return NotFound("The requested file does not exist on the server.");
        }





    }
}
