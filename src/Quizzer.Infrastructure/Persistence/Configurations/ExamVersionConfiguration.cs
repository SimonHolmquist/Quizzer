using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Quizzer.Domain.Exams;

namespace Quizzer.Infrastructure.Persistence.Configurations;

public sealed class ExamVersionConfiguration : IEntityTypeConfiguration<ExamVersion>
{
    public void Configure(EntityTypeBuilder<ExamVersion> builder)
    {
        builder.ToTable("ExamVersions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Notes)
            .IsRequired();

        builder.HasIndex(x => x.ExamId);

        builder.HasOne(x => x.Exam)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.ExamId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
