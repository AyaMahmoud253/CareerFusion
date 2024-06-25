using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class TelephoneInterviewQuestion
    {
        [Key]
        public int QuestionId { get; set; }

        public int PostId { get; set; }

        [ForeignKey("PostId")]
        public virtual Post Post { get; set; }


        [Required]
        public string Question { get; set; }

       
    }
}
