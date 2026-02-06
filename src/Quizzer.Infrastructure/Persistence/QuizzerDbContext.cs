using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;
using Quizzer.Domain.Exams;
using Quizzer.Domain.Study;

namespace Quizzer.Infrastructure.Persistence;

public sealed class QuizzerDbContext(DbContextOptions<QuizzerDbContext> options) : DbContext(options), IQuizzerDbContext
{
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamVersion> ExamVersions => Set<ExamVersion>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();
    public DbSet<QuestionStats> QuestionStats => Set<QuestionStats>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.ApplyConfigurationsFromAssembly(typeof(QuizzerDbContext).Assembly);

        base.OnModelCreating(mb);
    }
}
