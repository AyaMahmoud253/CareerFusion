namespace Web_API.Models
{
    public class ReportRecipientDTO
    {
        public int ReportId { get; set; }
        public string UserId { get; set; } // Foreign key referencing Candidate's ApplicationUser table
        public bool IsRead { get; set; }
        public bool IsAccepted { get; set; }
    }
}
