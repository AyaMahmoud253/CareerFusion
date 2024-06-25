using System.Threading.Tasks;
using Web_API.services;

namespace Web_API.Services
{
    public interface ICVUploadService // Changed interface name to ICVUploadService
    {
        Task<string> UploadFileAsync(int postId, string userId, IFormFile file);
        Task<IEnumerable<string>> GetCVPathsForPostAsync(int postId);
        Task<bool> UpdateScreeningStatusAsync(int postId, List<string> screenedEmails);
        Task<IEnumerable<object>> GetScreenedUsersAsync(int postId);
        Task<ServiceResult> SetTelephoneInterviewDateForPostAsync(int id, int postId, DateTime interviewDate);
        Task<ServiceResult> GetTelephoneInterviewDateForPostAsync(int id, int postId);
        Task<ServiceResult> ToggleTelephoneInterviewForPostCVAsync(int postId, int postCVId);
        Task<IEnumerable<object>> GetRecordsWithTelephoneInterviewPassedAsync(int postId);
        Task<MemoryStream> ExportRecordsWithTelephoneInterviewPassedToExcelAsync(int postId);
        Task<ServiceResult> SetTechnicalInterviewDateAsync(int postCVId, int postId, DateTime? technicalAssessmentDate, DateTime? physicalInterviewDate);
        Task<ServiceResult> GetTechnicalAssessmentDateAsync(int postCVId, int postId);
        Task<ServiceResult> GetPhysicalInterviewDateAsync(int postCVId, int postId);
        Task<ServiceResult> ToggleTechnicalInterviewForPostCVAsync(int postId, int postCVId, bool passed, string hrMessage);
        Task<IEnumerable<object>> GetRecordsWithTechnicalInterviewPassedAsync(int postId);
        Task<MemoryStream> ExportRecordsWithTechnicalInterviewPassedToExcelAsync(int postId);






    }
}
