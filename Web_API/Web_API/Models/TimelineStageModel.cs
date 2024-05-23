using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
namespace Web_API.Models
{
    public class TimelineStageModel
    {
        public int StageId { get; set; }
        public string? Description { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public bool Status { get; set; } 

    }

}
