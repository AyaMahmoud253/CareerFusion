using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class Course
    {
        [Key]
        public int CourseId { get; set; }

        public string CourseName { get; set; }

        // ... other properties as needed

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }
  }
