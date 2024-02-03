namespace Web_API.Models
{
    public class AuthModel
    {
        public string? Message { get; set; } // Adding '?' to indicate nullable string
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public List<string>? Roles { get; set; }
        public string? Token { get; set; }
        public DateTime ExpiresOn { get; set; }
        public string? UserId { get; set; }
    }
}
