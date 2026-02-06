namespace Quizzer.Domain.Exams;

public sealed class Option
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid QuestionId { get; set; }

    // Estable a través de versiones si la opción “equivale”
    public Guid OptionKey { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = "";
    public int OrderIndex { get; set; }
    public bool IsCorrect { get; set; }
}
