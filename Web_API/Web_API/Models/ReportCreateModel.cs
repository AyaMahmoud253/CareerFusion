using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class ReportCreateModel
    {
        [Key]
        public int ReportId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string HRUserId { get; set; } // Foreign key referencing HR's ApplicationUser table

        [ForeignKey("HRUserId")]
        public ApplicationUser HRUser { get; set; } // Navigation property to HR's ApplicationUser


    }
}
