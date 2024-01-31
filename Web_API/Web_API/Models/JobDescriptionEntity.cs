using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_API.Models
{
    public class JobDescriptionEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Description { get; set; }

        // Foreign key to JobFormEntity
        public int JobFormEntityId { get; set; }

        [ForeignKey("JobFormEntityId")]
        public virtual JobFormEntity JobForm { get; set; }
    }
}
