using Web_API.Models;

namespace Web_API.services
{
    public interface IReportService
    {
        Task<int> CreateReportAsync(ReportCreateModelDTO model, string hrUserId);
        Task SendReportToRecipientAsync(int reportId, string recipientUserId);
        Task<List<ReportDTO>> GetReportsForUserAsync(string userId);
        Task<ServiceResult> ToggleReportAcceptanceAsync(int reportId, string userId, bool accept);
        Task<ServiceResult> ToggleReportReadAsync(int reportId, string userId, bool isRead);
    }
}
