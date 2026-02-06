namespace Quizzer.Domain.Exams;

public sealed class Question
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ExamVersionId { get; set; }

    // Estable a través de versiones (la app lo hereda al clonar)
    public Guid QuestionKey { get; set; } = Guid.NewGuid();

    public string Text { get; set; } = "";
    public string? Explanation { get; set; }
    public int OrderIndex { get; set; }
    public int? Difficulty { get; set; }

    public Guid CorrectOptionId { get; set; }
    public ICollection<Option> Options { get; set; } = [];
}
