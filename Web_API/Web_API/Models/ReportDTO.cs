namespace Web_API.Models
{
    public class ReportDTO
    {
        public int? ReportId { get; set; }
        public string? Title { get; set; }
        public string? Text { get; set; }
        public bool? IsAccepted { get; set; }
        public bool? IsRead { get; set; }
    }
}
