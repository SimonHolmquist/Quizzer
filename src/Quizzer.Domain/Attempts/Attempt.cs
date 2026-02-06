namespace Quizzer.Domain;

public sealed class Attempt
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ExamVersionId { get; set; }
    public DateTimeOffset StartedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? FinishedAt { get; set; }

    public int TotalCount { get; set; }
    public int CorrectCount { get; set; }
    public double ScorePercent { get; set; }
    public int DurationSeconds { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = new List<AttemptAnswer>();
}
