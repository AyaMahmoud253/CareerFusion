// Goal.cs
using System;

namespace Web_API.Models
{
    public class Goal
    {
        public int Id { get; set; }
        public string HRUserId { get; set; } // Associate goals with an HR user
        public string Description { get; set; }
        public int Score { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<UserGoalScore> UserGoalScores { get; set; }

    }
    public class GoalInputModel
    {
        //public string HRUserId { get; set; }
        public string Description { get; set; }
        public int Score { get; set; }
    }
    public class UserGoalScore
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int GoalId { get; set; }
        public int Score { get; set; }

        public Goal Goal { get; set; }
    }


}
