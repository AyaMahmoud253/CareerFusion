using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class ReportRecipient
    {
        [Key]
        public int ReportRecipientId { get; set; }
        public int ReportId { get; set; }
        public string UserId { get; set; } // Foreign key referencing Candidate's ApplicationUser table

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } // Navigation property to Candidate's ApplicationUser

        public bool IsRead { get; set; } // Flag to track if the report has been read by the recipient
        public bool IsAccepted { get; set; } // Flag to track if the report has been accepted by the recipient
    }
}
