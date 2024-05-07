namespace Web_API.Models
{
    public class PostModel
    {
        public int PostId{ get; set; }
        public string? Content { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string? UserId { get; set; }
    }
}
