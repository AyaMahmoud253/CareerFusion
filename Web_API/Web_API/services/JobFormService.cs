using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
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

    }
}
