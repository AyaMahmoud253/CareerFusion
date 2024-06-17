namespace Web_API.Models
{
    public class JobFormModel
    {
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? JobType { get; set; }
        public string? JobLocation { get; set; }

        public string? UserId { get; set; }


        // Collection of Job Skills
        public List<JobSkillModel>? JobSkills { get; set; }

        // Collection of Job Descriptions
        public List<JobDescriptionModel>? JobDescriptions { get; set; }

        // Collection of Job Responsibilities
        public List<JobResponsibilityModel>? JobResponsibilities { get; set; }
        public List<TelephoneInterviewQuestionModel> TelephoneInterviewQuestions { get; set; }




    }
}
