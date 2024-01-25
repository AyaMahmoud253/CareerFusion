using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class Skill
    {
        [Key]
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public string SkillLevel { get; set; } // Optional, depending on how you want to represent skill level

        // Foreign key to ApplicationUser
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }
    }

}
