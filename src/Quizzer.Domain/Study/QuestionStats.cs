namespace Quizzer.Domain.Study;

public sealed class QuestionStats
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // Estable por pregunta a travÃ©s de versiones
    public Guid QuestionKey { get; set; }

    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? LastCorrectAt { get; set; }

    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }

    // Spaced repetition (simplificado)
    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; } = 1;
    public DateTimeOffset? DueAt { get; set; }
}
