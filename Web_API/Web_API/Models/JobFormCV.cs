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
}
