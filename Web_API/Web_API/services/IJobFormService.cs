﻿using Microsoft.AspNetCore.Mvc;
using Web_API.Models;

namespace Web_API.services
{
    public interface IJobFormService
    {
        Task<ServiceResult> AddJobFormAsync(string userId, JobFormModel model);
        Task<IEnumerable<JobFormModel>> GetJobsByUserIdAsync(string userId);
        Task<JobFormModel> GetSpecificJobForUserAsync(string userId, int jobId);
        Task<ServiceResult> UpdateJobResponsibilityAsync(string userId, int jobId, int responsibilityId, JobResponsibilityModel updatedResponsibility);
        Task<ServiceResult> UpdateJobSkillAsync(string userId, int jobId, int skillId, JobSkillModel updatedSkill);
        Task<ServiceResult> UpdateJobDescriptionAsync(string userId, int jobId, int descriptionId, JobDescriptionModel updatedDescription);
        Task<ServiceResult> DeleteJobSkillAsync(string userId, int jobId, int skillId);
        Task<ServiceResult> DeleteJobDescriptionAsync(string userId, int jobId, int descriptionId);
        Task<ServiceResult> DeleteJobResponsibilityAsync(string userId, int jobId, int responsibilityId);

    }
}