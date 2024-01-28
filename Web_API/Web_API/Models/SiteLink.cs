using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class SiteLink
    {
        [Key]
        public int LinkId { get; set; }

        public string LinkUrl { get; set; }

        // Foreign key to ApplicationUser
        public string UserId { get; set; }

        // Navigation property to access the associated user
        public ApplicationUser User { get; set; }
    }
}
