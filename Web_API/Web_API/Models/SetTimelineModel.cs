using System.ComponentModel.DataAnnotations;

namespace Web_API.Models
{
    public class SetTimelineModel
    {
        public List<TimelineStageModel> Stages { get; set; }
        public string UserId { get; set; }
    }

}
