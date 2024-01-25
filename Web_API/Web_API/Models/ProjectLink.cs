using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class ProjectLink
    {
        [Key]
        public int ProjectLinkId { get; set; }
        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }

        // Foreign key to ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
