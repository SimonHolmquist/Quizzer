namespace Quizzer.Domain.Exams;

public sealed class Exam
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public ICollection<ExamVersion> Versions { get; set; } = [];
}
