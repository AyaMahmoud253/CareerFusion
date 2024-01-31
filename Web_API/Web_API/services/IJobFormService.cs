using Web_API.Models;

namespace Web_API.services
{
    public interface IJobFormService
    {
        Task<ServiceResult> AddJobFormAsync(string userId, JobFormModel model);
    }
}
