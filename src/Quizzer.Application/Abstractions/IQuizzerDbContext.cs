using Microsoft.EntityFrameworkCore;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Abstractions;

public interface IQuizzerDbContext
{
    DbSet<Exam> Exams { get; }
    DbSet<ExamVersion> ExamVersions { get; }
    DbSet<Question> Questions { get; }
    DbSet<Option> Options { get; }
    DbSet<Attempt> Attempts { get; }
    DbSet<AttemptAnswer> AttemptAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
