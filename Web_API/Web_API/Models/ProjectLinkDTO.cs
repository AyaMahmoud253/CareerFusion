namespace Web_API.Models
{
    public class ProjectLinkDTO
    {
        public int? ProjectLinkId { get; set; } // Nullable for new projects
        public string ProjectName { get; set; }
        public string ProjectUrl { get; set; }
    }
}
