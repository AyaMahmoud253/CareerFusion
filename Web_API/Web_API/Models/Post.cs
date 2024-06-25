using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class Post
    {
        [Key]
        public int PostId { get; set; }
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UserId { get; set; }
        // Foreign key to User
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        // Navigation property for related files uploaded with this post
        public virtual ICollection<PostFile> PostFiles { get; set; }

        // Navigation property for related pictures associated with this post
        public virtual ICollection<PostPicture> PostPictures { get; set; }
    }
}
