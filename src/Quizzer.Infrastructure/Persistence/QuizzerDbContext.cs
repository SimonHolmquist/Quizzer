using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
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

    static readonly ValueConverter<DateTimeOffset, long> DateTimeOffsetToUnixMs =
    new(v => v.ToUnixTimeMilliseconds(),
        v => DateTimeOffset.FromUnixTimeMilliseconds(v));

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Exam>().HasKey(x => x.Id);
        mb.Entity<Exam>().Property(x => x.CreatedAt).HasConversion(DateTimeOffsetToUnixMs);

        mb.Entity<ExamVersion>().HasKey(x => x.Id);
        mb.Entity<ExamVersion>().Property(x => x.CreatedAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<ExamVersion>().Property(x => x.PublishedAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<ExamVersion>()
            .HasOne<Exam>()
            .WithMany(e => e.Versions)
            .HasForeignKey(v => v.ExamId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Question>().HasKey(x => x.Id);
        mb.Entity<Question>()
            .HasMany(q => q.Options)
            .WithOne()
            .HasForeignKey(o => o.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Option>().HasKey(x => x.Id);

        mb.Entity<Attempt>().HasKey(x => x.Id);
        mb.Entity<Attempt>().Property(x => x.StartedAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<Attempt>().Property(x => x.FinishedAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<AttemptAnswer>().HasKey(x => new { x.AttemptId, x.QuestionId });

        mb.Entity<QuestionStats>().HasKey(x => x.Id);
        mb.Entity<QuestionStats>().Property(x => x.LastSeenAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<QuestionStats>().Property(x => x.LastCorrectAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<QuestionStats>().Property(x => x.DueAt).HasConversion(DateTimeOffsetToUnixMs);
        mb.Entity<QuestionStats>().HasIndex(x => x.QuestionKey);

        mb.Entity<Question>().HasIndex(x => x.QuestionKey);
        mb.Entity<Option>().HasIndex(x => x.OptionKey);

        mb.Entity<QuestionStats>().HasKey(x => x.Id);
        mb.Entity<QuestionStats>().HasIndex(x => x.QuestionKey).IsUnique();

        base.OnModelCreating(mb);
    }
}
