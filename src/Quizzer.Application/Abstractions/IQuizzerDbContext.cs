using Microsoft.EntityFrameworkCore;
using Quizzer.Domain;
using Quizzer.Domain.Exams;
using Quizzer.Domain.Study;

namespace Quizzer.Application.Abstractions;

public interface IQuizzerDbContext
{
    DbSet<Exam> Exams { get; }
    DbSet<ExamVersion> ExamVersions { get; }
    DbSet<Question> Questions { get; }
    DbSet<Option> Options { get; }
    DbSet<Attempt> Attempts { get; }
    DbSet<AttemptAnswer> AttemptAnswers { get; }
    DbSet<QuestionStats> QuestionStats { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
