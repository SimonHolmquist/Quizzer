using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Attempts.Queries;

public sealed record GetAttemptHistoryQuery(Guid ExamId) : IRequest<IReadOnlyList<AttemptHistoryItemDto>>;

public sealed record AttemptHistoryItemDto(
    Guid AttemptId,
    Guid ExamVersionId,
    int VersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    int TotalCount,
    int CorrectCount,
    double ScorePercent,
    int DurationSeconds);

public sealed class GetAttemptHistoryQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetAttemptHistoryQuery, IReadOnlyList<AttemptHistoryItemDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<AttemptHistoryItemDto>> Handle(GetAttemptHistoryQuery request, CancellationToken ct)
    {
        var exam = await _db.Exams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        var versions = await _db.ExamVersions.AsNoTracking()
            .Where(v => v.ExamId == exam.Id)
            .ToListAsync(ct);

        var versionMap = versions.ToDictionary(v => v.Id, v => v.VersionNumber);

        var attempts = await _db.Attempts.AsNoTracking()
            .Where(a => versionMap.Keys.Contains(a.ExamVersionId))
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(ct);

        return [.. attempts.Select(a =>
        {
            var versionNumber = versionMap.GetValueOrDefault(a.ExamVersionId);
            return new AttemptHistoryItemDto(
                a.Id,
                a.ExamVersionId,
                versionNumber,
                a.StartedAt,
                a.FinishedAt,
                a.TotalCount,
                a.CorrectCount,
                a.ScorePercent,
                a.DurationSeconds);
        })];
    }
}
