namespace Web_API.Models
{
    public class SkillDTO
    {
        public int? SkillId { get; set; } // Nullable for new skills
        public string SkillName { get; set; }
        public string SkillLevel { get; set; } // Optional
    }
}
