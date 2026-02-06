using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain.Study;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class QuestionStatsConfiguration : IEntityTypeConfiguration<QuestionStats>
{
    public void Configure(EntityTypeBuilder<QuestionStats> builder)
    {
        builder.ToTable("QuestionStats");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.QuestionKey);
    }
}
