using System.ComponentModel.DataAnnotations;
using Web_API.Models;

public class JobFormCV
{
    [Key]
    public int Id { get; set; }

    // Foreign key to the job form
    public int JobFormId { get; set; }
    public virtual JobFormEntity JobForm { get; set; }

    // Foreign key to the user
    public string UserId { get; set; }

    // File path of the CV
    public string FilePath { get; set; }
    // Flag to indicate if the score is above 70
    public bool IsScoreAbove70 { get; set; }
    public bool isTelephoneInterviewPassed { get; set; }
    public bool IsTechnicalInterviewPassed { get; set; } // New property for technical interview status

    // New properties for interview dates
    public DateTime? TechnicalAssessmentDate { get; set; }
    public DateTime? PhysicalInterviewDate { get; set; }
    public DateTime? TelephoneInterviewDate { get; set; } // New property for telephone interview date

}
