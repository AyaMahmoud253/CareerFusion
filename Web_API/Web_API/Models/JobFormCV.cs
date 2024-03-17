using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class JobFormCV
    {
        [Key]
        public int Id { get; set; }

        // Foreign key to the job form
        public int JobFormId { get; set; }
        public virtual JobFormEntity JobForm { get; set; }

        // File path of the CV
        public string FilePath { get; set; }
    }
}
