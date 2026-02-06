namespace Quizzer.Domain;

public sealed class AttemptAnswer
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid QuestionKey { get; set; }

    public Guid SelectedOptionId { get; set; }
    public Guid SelectedOptionKey { get; set; }

    public bool IsCorrect { get; set; }
    public DateTimeOffset AnsweredAt { get; set; } = DateTimeOffset.UtcNow;
    public int SecondsSpent { get; set; }
    public bool FlaggedDoubt { get; set; }
}
