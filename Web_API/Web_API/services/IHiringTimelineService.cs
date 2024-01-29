using Web_API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Web_API.services
{
    public interface IHiringTimelineService
    {
        Task<ServiceResult> SetHiringTimelineAsync(SetTimelineModel model);
        Task<IEnumerable<TimelineStageModel>> GetTimelinesForUserAsync(string userId);
        Task<ServiceResult> UpdateTimelineStageAsync(string userId, int stageId, TimelineStageModel updatedStage);
        Task<ServiceResult> DeleteTimelineStageAsync(string userId, int stageId);

    }

}
