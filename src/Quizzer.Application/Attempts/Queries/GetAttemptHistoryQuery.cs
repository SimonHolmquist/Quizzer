using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Attempts.Queries;

public sealed record GetAttemptHistoryQuery(Guid? ExamId = null) : IRequest<IReadOnlyList<AttemptHistoryItemDto>>;

public sealed record AttemptHistoryItemDto(
    Guid AttemptId,
    Guid ExamId,
    string ExamName,
    int VersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    double ScorePercent);

public sealed class GetAttemptHistoryQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetAttemptHistoryQuery, IReadOnlyList<AttemptHistoryItemDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<AttemptHistoryItemDto>> Handle(GetAttemptHistoryQuery request, CancellationToken ct)
    {
        var versions = await _db.ExamVersions
            .AsNoTracking()
            .Include(v => v.Exam)
            .ToListAsync(ct);

        if (request.ExamId is not null)
            versions = versions.Where(v => v.ExamId == request.ExamId).ToList();

        var versionIds = versions.Select(v => v.Id).ToList();

        var attempts = await _db.Attempts
            .AsNoTracking()
            .Where(a => versionIds.Contains(a.ExamVersionId))
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(ct);

        return attempts.Select(a =>
        {
            var version = versions.First(v => v.Id == a.ExamVersionId);
            return new AttemptHistoryItemDto(
                a.Id,
                version.ExamId,
                version.Exam?.Name ?? "",
                version.VersionNumber,
                a.StartedAt,
                a.FinishedAt,
                a.ScorePercent);
        }).ToList();
    }
}
