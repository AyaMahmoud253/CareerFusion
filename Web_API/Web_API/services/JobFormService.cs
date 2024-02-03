using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web_API.Models;
using Web_API.services;

namespace Web_API.Services
{
    public class JobFormService : IJobFormService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobFormService(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResult> AddJobFormAsync(string userId, JobFormModel model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "HR")))
            {
                return new ServiceResult { Success = false, Message = "User is not in the HR role." };
            }

            var jobForm = new JobFormEntity
            {
                JobTitle = model.JobTitle,
                JobType = model.JobType,
                JobLocation = model.JobLocation,
                UserId = userId
            };

            _context.JobForms.Add(jobForm);
            await _context.SaveChangesAsync(); // Save initially to generate JobForm ID

            var skillIds = new List<int>();
            var descriptionIds = new List<int>();
            var responsibilityIds = new List<int>();

            // Add Job Skills
            if (model.JobSkills != null)
            {
                foreach (var skill in model.JobSkills)
                {
                    var jobSkill = new JobSkillEntity { SkillName = skill.SkillName, JobFormEntityId = jobForm.Id };
                    _context.JobSkills.Add(jobSkill);
                }
                await _context.SaveChangesAsync(); // Save to generate Skill IDs
                skillIds.AddRange(jobForm.JobSkills.Select(js => js.Id)); // Collect Skill IDs
            }

            // Add Job Descriptions
            if (model.JobDescriptions != null)
            {
                foreach (var description in model.JobDescriptions)
                {
                    var jobDescription = new JobDescriptionEntity { Description = description.Description, JobFormEntityId = jobForm.Id };
                    _context.JobDescriptions.Add(jobDescription);
                }
                await _context.SaveChangesAsync(); // Save to generate Description IDs
                descriptionIds.AddRange(jobForm.JobDescriptions.Select(jd => jd.Id)); // Collect Description IDs
            }

            // Add Job Responsibilities
            if (model.JobResponsibilities != null)
            {
                foreach (var responsibility in model.JobResponsibilities)
                {
                    var jobResponsibility = new JobResponsibilityEntity { Responsibility = responsibility.Responsibility, JobFormEntityId = jobForm.Id };
                    _context.JobResponsibilities.Add(jobResponsibility);
                }
                await _context.SaveChangesAsync(); // Save to generate Responsibility IDs
                responsibilityIds.AddRange(jobForm.JobResponsibilities.Select(jr => jr.Id)); // Collect Responsibility IDs
            }

            // Return Success with all IDs
            return new ServiceResult
            {
                Success = true,
                Message = "Job form and related entities added successfully.",
                Payload = new
                {
                    JobId = jobForm.Id,
                    SkillIds = skillIds,
                    DescriptionIds = descriptionIds,
                    ResponsibilityIds = responsibilityIds
                }
            };
        }

        public async Task<IEnumerable<JobFormModel>> GetJobsByUserIdAsync(string userId)
        {
            // Check if the user exists in the system
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                // Return an empty list to indicate that no user was found for the provided ID
                return Enumerable.Empty<JobFormModel>();
            }

            // Query the JobForms table to retrieve only the specified columns for the given UserId
            var jobForms = await _context.JobForms
                .Where(jf => jf.UserId == userId)
                .Select(jf => new JobFormModel
                {
                    JobTitle = jf.JobTitle,
                    JobType = jf.JobType,
                    JobLocation = jf.JobLocation,
                   // UserId = jf.UserId  // Include if you want to return the UserId as well
                })
                .ToListAsync();  // Execute the query and convert the result to a List

            // Return the list of JobFormModel objects
            return jobForms;
        }
        public async Task<JobFormModel> GetSpecificJobForUserAsync(string userId, int jobId)
        {
            var jobForm = await _context.JobForms
                .Where(jf => jf.UserId == userId && jf.Id == jobId)
                .Select(jf => new JobFormModel
                {
                    JobTitle = jf.JobTitle,
                    JobType = jf.JobType,
                    JobLocation = jf.JobLocation,
                    UserId = jf.UserId,
                    JobSkills = jf.JobSkills.Select(js => new JobSkillModel
                    {
                        SkillName = js.SkillName,
                        // Map other properties of JobSkill to JobSkillModel
                    }).ToList(),
                    JobDescriptions = jf.JobDescriptions.Select(jd => new JobDescriptionModel
                    {
                        Description = jd.Description,
                        // Map other properties of JobDescription to JobDescriptionModel
                    }).ToList(),
                    JobResponsibilities = jf.JobResponsibilities.Select(jr => new JobResponsibilityModel
                    {
                        Responsibility = jr.Responsibility,
                        // Map other properties of JobResponsibility to JobResponsibilityModel
                    }).ToList(),
                    // Include additional properties as needed
                })
                .FirstOrDefaultAsync();

            return jobForm;
        }
        public async Task<ServiceResult> UpdateJobResponsibilityAsync(string userId, int jobId, int responsibilityId, JobResponsibilityModel updatedResponsibility)
        {
            // Find the job form first to ensure it belongs to the given user and has the specified job ID
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobResponsibilities)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            // Find the specific responsibility within the job form
            var responsibilityEntity = jobForm.JobResponsibilities
                .FirstOrDefault(jr => jr.Id == responsibilityId);

            if (responsibilityEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job responsibility not found." };
            }

            // Update the responsibility
            responsibilityEntity.Responsibility = updatedResponsibility.Responsibility;

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job responsibility updated successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }


        public async Task<ServiceResult> UpdateJobSkillAsync(string userId, int jobId, int skillId, JobSkillModel updatedSkill)
        {
            // Find the job form to ensure it belongs to the given user and has the specified job ID
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobSkills)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            // Find the specific skill within the job form
            var skillEntity = jobForm.JobSkills.FirstOrDefault(js => js.Id == skillId);

            if (skillEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job skill not found." };
            }

            // Update the skill
            skillEntity.SkillName = updatedSkill.SkillName;

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job skill updated successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> UpdateJobDescriptionAsync(string userId, int jobId, int descriptionId, JobDescriptionModel updatedDescription)
        {
            // Find the job form to ensure it belongs to the given user and has the specified job ID
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobDescriptions)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            // Find the specific description within the job form
            var descriptionEntity = jobForm.JobDescriptions.FirstOrDefault(jd => jd.Id == descriptionId);

            if (descriptionEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job description not found." };
            }

            // Update the description
            descriptionEntity.Description = updatedDescription.Description;

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job description updated successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
        public async Task<ServiceResult> DeleteJobSkillAsync(string userId, int jobId, int skillId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobSkills)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            var skillEntity = jobForm.JobSkills.FirstOrDefault(js => js.Id == skillId);

            if (skillEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job skill not found." };
            }

            _context.JobSkills.Remove(skillEntity);

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job skill deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

        public async Task<ServiceResult> DeleteJobDescriptionAsync(string userId, int jobId, int descriptionId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobDescriptions)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            var descriptionEntity = jobForm.JobDescriptions.FirstOrDefault(jd => jd.Id == descriptionId);

            if (descriptionEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job description not found." };
            }

            _context.JobDescriptions.Remove(descriptionEntity);

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job description deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }
        public async Task<ServiceResult> DeleteJobResponsibilityAsync(string userId, int jobId, int responsibilityId)
        {
            var jobForm = await _context.JobForms
                .Include(jf => jf.JobResponsibilities)
                .FirstOrDefaultAsync(jf => jf.Id == jobId && jf.UserId == userId);

            if (jobForm == null)
            {
                return new ServiceResult { Success = false, Message = "Job form not found or does not belong to the user." };
            }

            var responsibilityEntity = jobForm.JobResponsibilities.FirstOrDefault(jr => jr.Id == responsibilityId);

            if (responsibilityEntity == null)
            {
                return new ServiceResult { Success = false, Message = "Job responsibility not found." };
            }

            _context.JobResponsibilities.Remove(responsibilityEntity);

            try
            {
                await _context.SaveChangesAsync();
                return new ServiceResult { Success = true, Message = "Job responsibility deleted successfully." };
            }
            catch (Exception ex)
            {
                return new ServiceResult { Success = false, Message = $"An error occurred: {ex.Message}" };
            }
        }

       





    }
}
