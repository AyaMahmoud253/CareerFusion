namespace Web_API.Models
{
    public class UserProfileUpdateDTO
    {
     
        public string? Title { get; set; }
        public string ?ProfilePicturePath { get; set; }
        public int? FollowersCount { get; set; }
        public string? Description { get; set; } // Add Description property
        public string? Address { get; set; }    // Add Address property

        // Assuming CourseDTO is a class that represents the data you want to update for a course
        public ICollection<CourseDTO>? Courses { get; set; }
        public ICollection<ProjectLinkDTO>? ProjectLinks { get; set; }
        public ICollection<SkillDTO>? Skills { get; set; }
        public ICollection<SiteLinkDTO>? SiteLinks { get; set; } // Add SiteLinkDTO collection


    }



}
