using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.ToTable("AttemptAnswers");

        builder.HasKey(x => new { x.AttemptId, x.QuestionId });

        builder.HasOne<Attempt>()
            .WithMany(a => a.Answers)
            .HasForeignKey(x => x.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
