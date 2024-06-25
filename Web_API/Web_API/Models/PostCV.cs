using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class PostCV
    {
        [Key]
        public int PostCVId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post UploadedPost { get; set; }
        public string UserId { get; set; } // Foreign key to user

        public string FilePath { get; set; }
        public bool PassedScreening { get; set; }

        public bool IsTelephoneInterviewPassed { get; set; } // New property for telephone interview status
        public DateTime? TelephoneInterviewDate { get; set; } // New property for telephone interview date

        public bool IsTechnicalInterviewPassed { get; set; } // New property for technical interview status

        public DateTime? TechnicalAssessmentDate { get; set; } // New property for technical assessment date

        public DateTime? PhysicalInterviewDate { get; set; } // New property for physical interview date

    }
}
