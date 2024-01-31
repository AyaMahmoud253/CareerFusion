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
            // Check if the provided userId is valid and belongs to a user in the HR role
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
                UserId = userId // Use the provided userId
                                // Map other job form properties as needed
            };

            // Add Job Form to the context and save changes to get the generated Id
            _context.JobForms.Add(jobForm);
            await _context.SaveChangesAsync();

            // Now use the generated Id for setting foreign keys in related entities

            // Add Job Skills
            if (model.JobSkills != null)
            {
                foreach (var skillModel in model.JobSkills)
                {
                    var jobSkill = new JobSkillEntity
                    {
                        SkillName = skillModel.SkillName,
                        JobFormEntityId = jobForm.Id
                        // Map other job skill properties as needed
                    };

                    _context.JobSkills.Add(jobSkill); // Add directly to the context
                }
            }

            // Add Job Descriptions
            if (model.JobDescriptions != null)
            {
                foreach (var descriptionModel in model.JobDescriptions)
                {
                    var jobDescription = new JobDescriptionEntity
                    {
                        Description = descriptionModel.Description,
                        JobFormEntityId = jobForm.Id
                        // Map other job description properties as needed
                    };

                    _context.JobDescriptions.Add(jobDescription); // Add directly to the context
                }
            }

            // Add Job Responsibilities
            if (model.JobResponsibilities != null)
            {
                foreach (var responsibilityModel in model.JobResponsibilities)
                {
                    var jobResponsibility = new JobResponsibilityEntity
                    {
                        Responsibility = responsibilityModel.Responsibility,
                        JobFormEntityId = jobForm.Id
                        // Map other job responsibility properties as needed
                    };

                    _context.JobResponsibilities.Add(jobResponsibility); // Add directly to the context
                }
            }

            await _context.SaveChangesAsync(); // Save changes after adding related entities

            return new ServiceResult { Success = true, Message = "Job form added successfully." };
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
