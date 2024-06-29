public class EvaluationQuestion
{
    public int Id { get; set; }
    public string HRId { get; set; }
    public string Question { get; set; }
    public int DefaultScore { get; set; }
}

public class UserQuestionScore
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public int EvaluationQuestionId { get; set; }
    public int Score { get; set; }

    public virtual EvaluationQuestion EvaluationQuestion { get; set; }
}
public class UserEvaluation
{
    public string UserId { get; set; }
    public double OverallScore { get; set; }
    public List<EvaluationQuestion> Questions { get; set; }
    public string ComparisonResult { get; set; }
}
public class CreateQuestionModel
{
    public string Question { get; set; }
    public int DefaultScore { get; set; }
}
public class UserScoreModel
{
    public string UserId { get; set; }
    public int QuestionId { get; set; }
    public int Score { get; set; }
}
