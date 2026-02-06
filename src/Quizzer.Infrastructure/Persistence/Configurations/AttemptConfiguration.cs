using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class AttemptConfiguration : IEntityTypeConfiguration<Attempt>
{
    public void Configure(EntityTypeBuilder<Attempt> builder)
    {
        builder.ToTable("Attempts");

        builder.HasKey(x => x.Id);

        builder.HasMany(x => x.Answers)
            .WithOne()
            .HasForeignKey(x => x.AttemptId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
