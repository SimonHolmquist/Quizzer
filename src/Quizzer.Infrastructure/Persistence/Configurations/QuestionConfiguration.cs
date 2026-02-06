using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain.Exams;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired();

        builder.HasIndex(x => x.ExamVersionId);
        builder.HasIndex(x => x.QuestionKey);

        builder.HasOne<ExamVersion>()
            .WithMany(x => x.Questions)
            .HasForeignKey(x => x.ExamVersionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Options)
            .WithOne()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
