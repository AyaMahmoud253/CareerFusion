using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class JobFormEntity
    {
        [Key]
        public int Id { get; set; }
        public string JobTitle { get; set; }


        public string JobType { get; set; }

       
        public string JobLocation { get; set; }

    
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // Collection of Job Skills
        public virtual ICollection<JobSkillEntity> JobSkills { get; set; }

        // New collection for Job Description
        public virtual ICollection<JobDescriptionEntity> JobDescriptions { get; set; }

        // New collection for Job Responsibilities
        public virtual ICollection<JobResponsibilityEntity> JobResponsibilities { get; set; }
        // Collection for storing CVs associated with job forms
        public virtual ICollection<JobFormCV> JobFormCVs { get; set; }
        // Collection for telephone interview questions
        public virtual ICollection<TelephoneInterviewQuestionEntity> TelephoneInterviewQuestions { get; set; }
    }
}
