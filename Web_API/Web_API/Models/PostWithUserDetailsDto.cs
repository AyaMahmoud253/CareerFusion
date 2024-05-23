namespace Web_API.Models
{
    public class PostWithUserDetailsDto
    {
        public int PostId { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserProfilePicturePath { get; set; }
    }

}
