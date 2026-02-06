using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Infrastructure.Persistence;

public sealed class QuizzerDbContext(DbContextOptions options) : DbContext(options), IQuizzerDbContext
{
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<ExamVersion> ExamVersions => Set<ExamVersion>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Option> Options => Set<Option>();
    public DbSet<Attempt> Attempts => Set<Attempt>();
    public DbSet<AttemptAnswer> AttemptAnswers => Set<AttemptAnswer>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<AttemptAnswer>().HasKey(x => new { x.AttemptId, x.QuestionId });

        mb.Entity<Question>()
          .HasMany(q => q.Options)
          .WithOne()
          .HasForeignKey(o => o.QuestionId)
          .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<ExamVersion>()
          .HasMany(v => v.Questions)
          .WithOne()
          .HasForeignKey(q => q.ExamVersionId)
          .OnDelete(DeleteBehavior.Cascade);

        // Índices útiles para reportes
        mb.Entity<Question>().HasIndex(q => q.QuestionKey);
        mb.Entity<Option>().HasIndex(o => o.OptionKey);
        mb.Entity<Attempt>().HasIndex(a => a.ExamVersionId);
    }
}
