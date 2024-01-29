using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Web_API.helpers;
using Web_API.Models;

namespace Web_API.services
{
    public class HiringTimelineService : IHiringTimelineService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HiringTimelineService(ApplicationDBContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<ServiceResult> SetHiringTimelineAsync(SetTimelineModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "HR")))
            {
                return new ServiceResult { Success = false, Message = "User is not in the HR role." };
            }

            foreach (var stage in model.Stages)
            {
                var timelineStage = new TimelineStageEntity
                {
                    Description = stage.Description,
                    StartTime = stage.StartTime,
                    EndTime = stage.EndTime,
                    UserId = model.UserId
                };
                _context.TimelineStages.Add(timelineStage);
            }

            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Timeline set successfully." };
        }
        public async Task<ServiceResult> UpdateTimelineStageAsync(string userId, int stageId, TimelineStageModel updatedStage)
        {
            var timelineStage = await _context.TimelineStages
                .FirstOrDefaultAsync(ts => ts.Id == stageId && ts.UserId == userId);

            if (timelineStage == null)
            {
                return new ServiceResult { Success = false, Message = "Timeline stage not found or user not authorized to update this timeline stage." };
            }

            // Optionally, ensure that the user is allowed to update the timeline stage
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !(await _userManager.IsInRoleAsync(user, "HR")))
            {
                return new ServiceResult { Success = false, Message = "User is not in the HR role or not found." };
            }

            // Update the timeline stage with the new values only if they are provided
            if (!string.IsNullOrWhiteSpace(updatedStage.Description))
            {
                timelineStage.Description = updatedStage.Description;
            }
            if (updatedStage.StartTime.HasValue)
            {
                timelineStage.StartTime = updatedStage.StartTime.Value;
            }
            if (updatedStage.EndTime.HasValue)
            {
                timelineStage.EndTime = updatedStage.EndTime.Value;
            }

            // Save the changes in the database
            _context.TimelineStages.Update(timelineStage);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Timeline stage updated successfully." };
        }

        public async Task<ServiceResult> DeleteTimelineStageAsync(string userId, int stageId)
        {
            var timelineStage = await _context.TimelineStages
                .FirstOrDefaultAsync(ts => ts.Id == stageId && ts.UserId == userId);

            if (timelineStage == null)
            {
                return new ServiceResult { Success = false, Message = "Timeline stage not found or user not authorized to delete this timeline stage." };
            }

            _context.TimelineStages.Remove(timelineStage);
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = "Timeline stage deleted successfully." };
        }
        public async Task<IEnumerable<TimelineStageModel>> GetTimelinesForUserAsync(string userId)
        {
            // Retrieve the stages from the database
            var timelineStagesEntities = await _context.TimelineStages
                .Where(ts => ts.UserId == userId)
                .ToListAsync();

            // Check each retrieved entity before converting to model
            foreach (var stageEntity in timelineStagesEntities)
            {
                if (stageEntity.Description == null ||
                    stageEntity.StartTime == DateTime.MinValue ||
                    stageEntity.EndTime == DateTime.MinValue)
                {
                    // Log the error or handle the case where the data is not as expected
                    // For example, you might throw an exception or continue
                    throw new InvalidOperationException("Invalid timeline stage data.");
                }
            }

            // Convert entities to models
            var timelineStagesModels = timelineStagesEntities.Select(ts => new TimelineStageModel
            {
                Description = ts.Description,
                StartTime = ts.StartTime,
                EndTime = ts.EndTime
            }).ToList();

            return timelineStagesModels;
        }

    }

}
