using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class TelephoneInterviewQuestionEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Question { get; set; }

        [Required]
        public string JobTitle { get; set; }

        public int JobFormEntityId { get; set; }

        [ForeignKey("JobFormEntityId")]
        public virtual JobFormEntity JobForm { get; set; }
    }
}
