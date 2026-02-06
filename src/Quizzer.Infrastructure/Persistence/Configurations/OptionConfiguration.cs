using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain.Exams;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class OptionConfiguration : IEntityTypeConfiguration<Option>
{
    public void Configure(EntityTypeBuilder<Option> builder)
    {
        builder.ToTable("Options");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Text)
            .IsRequired();

        builder.HasIndex(x => x.OptionKey);
        builder.HasIndex(x => x.QuestionId);

        builder.HasOne<Question>()
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
