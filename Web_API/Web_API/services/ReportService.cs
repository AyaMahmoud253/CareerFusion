using Microsoft.EntityFrameworkCore;
using Web_API.Models;

namespace Web_API.services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDBContext _context;

        public ReportService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<int> CreateReportAsync(ReportCreateModelDTO model, string hrUserId)
        {
            try
            {
                var report = new ReportCreateModel
                {
                    Title = model.Title,
                    Text = model.Text,
                    HRUserId = hrUserId // Use the HR user ID from the URL
                };

                _context.Reports.Add(report);
                await _context.SaveChangesAsync();

                return report.ReportId;
            }
            catch (DbUpdateException ex)
            {
                // Log the exception or handle it as appropriate
                throw new Exception("Failed to create report.", ex);
            }
        }


        public async Task SendReportToRecipientAsync(int reportId, string recipientUserId)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null)
            {
                throw new ArgumentException($"Report with ID {reportId} not found.");
            }

            var recipient = new ReportRecipient
            {
                ReportId = reportId,
                UserId = recipientUserId,
                IsRead = false,
                IsAccepted = false // Initialize IsAccepted as false when sending to recipient
            };

            _context.ReportRecipients.Add(recipient);
            await _context.SaveChangesAsync();
        }

        public async Task<List<ReportDTO>> GetReportsForUserAsync(string userId)
        {
            var reportRecipients = await _context.ReportRecipients
                .Where(rr => rr.UserId == userId)
                .Select(rr => new
                {
                    rr.ReportId,
                    rr.IsRead,
                    rr.IsAccepted,
                    Report = _context.Reports.FirstOrDefault(r => r.ReportId == rr.ReportId)
                })
                .ToListAsync();

            var reports = reportRecipients.Select(rr => new ReportDTO
            {
                ReportId = rr.ReportId,
                Title = rr.Report.Title,
                Text = rr.Report.Text,
                IsAccepted = rr.IsAccepted,
                IsRead = rr.IsRead
            }).ToList();

            // Mark reports as read when fetched
            foreach (var reportRecipient in reportRecipients)
            {
                if (!reportRecipient.IsRead)
                {
                    var recipient = await _context.ReportRecipients
                        .FirstOrDefaultAsync(rr => rr.ReportId == reportRecipient.ReportId && rr.UserId == userId);

                    if (recipient != null)
                    {
                        recipient.IsRead = true;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return reports;
        }


        public async Task<ServiceResult> ToggleReportAcceptanceAsync(int reportId, string userId, bool accept)
        {
            var reportRecipient = await _context.ReportRecipients
                .FirstOrDefaultAsync(rr => rr.ReportId == reportId && rr.UserId == userId);

            if (reportRecipient == null)
            {
                throw new ArgumentException($"Report recipient with ID {reportId} and user ID {userId} not found.");
            }

            reportRecipient.IsAccepted = accept;
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = accept ? "Report accepted successfully." : "Report rejected successfully." };
        }

        public async Task<ServiceResult> ToggleReportReadAsync(int reportId, string userId, bool isRead)
        {
            var reportRecipient = await _context.ReportRecipients
                .FirstOrDefaultAsync(rr => rr.ReportId == reportId && rr.UserId == userId);

            if (reportRecipient == null)
            {
                throw new ArgumentException($"Report recipient with ID {reportId} and user ID {userId} not found.");
            }

            reportRecipient.IsRead = isRead;
            await _context.SaveChangesAsync();

            return new ServiceResult { Success = true, Message = isRead ? "Report marked as read successfully." : "Report marked as unread successfully." };
        }
    }

}
