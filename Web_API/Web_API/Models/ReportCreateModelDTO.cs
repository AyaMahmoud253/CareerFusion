namespace Web_API.Models
{
    public class ReportCreateModelDTO
    {
        public string? Title { get; set; }
        public string? Text { get; set; }
        public string? HRUserId { get; set; } // Foreign key referencing HR's ApplicationUser table
    }
}
