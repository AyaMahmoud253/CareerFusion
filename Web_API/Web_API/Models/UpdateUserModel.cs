using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class UpdateUserModel
    {
        [StringLength(100)]
        public string UserName { get; set; }

        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(128)]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }
}
