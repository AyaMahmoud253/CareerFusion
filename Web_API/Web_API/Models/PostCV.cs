using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class PostCV
    {
        [Key]
        public int PostCVId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post UploadedPost { get; set; }

        public string FilePath { get; set; }
    }
}
