using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class RegisterModel
    {
        [Required, StringLength(100)]
        public string UserName { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, StringLength(128)]
        public string Email { get; set; }

        [Required, StringLength(256)]
        public string Password { get; set; }

        [StringLength(20)] // Define appropriate length
        public string PhoneNumber { get; set; }

        public RegisterModel()
        {
            UserName = string.Empty;
            FullName = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            PhoneNumber= string.Empty;
            // Initialize other non-nullable properties if needed
        }
    }
}
