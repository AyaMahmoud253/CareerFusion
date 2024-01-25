namespace Web_API.Models
{
    public class UserProfileUpdateDTO
    {
     
        public string? Title { get; set; }
        public string ?ProfilePicturePath { get; set; }
        public int? FollowersCount { get; set; }

        // Assuming CourseDTO is a class that represents the data you want to update for a course
        public ICollection<CourseDTO>? Courses { get; set; }
        public ICollection<ProjectLinkDTO>? ProjectLinks { get; set; }
        public ICollection<SkillDTO>? Skills { get; set; }
       
    }
   


}
