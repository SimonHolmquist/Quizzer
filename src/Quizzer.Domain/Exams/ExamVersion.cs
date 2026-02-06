using Quizzer.Domain.Exams;

namespace Quizzer.Domain.Exams;

public sealed class ExamVersion
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ExamId { get; set; }
    public int VersionNumber { get; set; }
    public VersionStatus Status { get; set; }
    public string Notes { get; set; } = "";
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PublishedAt { get; set; }

    public Exam? Exam { get; set; }
    public ICollection<Question> Questions { get; set; } = [];
}
