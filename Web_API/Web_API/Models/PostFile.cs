using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class PostFile
    {
        [Key]
        public int PostFileId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post UploadedPost { get; set; }

        public string FilePath { get; set; }

    }
}
