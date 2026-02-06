using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetExamDashboardQuery : IRequest<IReadOnlyList<ExamDashboardItemDto>>;

public sealed record ExamDashboardItemDto(
    Guid ExamId,
    string Name,
    int TotalAttempts,
    double AverageScorePercent,
    DateTimeOffset? LastAttemptAt);

public sealed class GetExamDashboardQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetExamDashboardQuery, IReadOnlyList<ExamDashboardItemDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<ExamDashboardItemDto>> Handle(GetExamDashboardQuery request, CancellationToken ct)
    {
        var exams = await _db.Exams
            .AsNoTracking()
            .Where(e => !e.IsDeleted)
            .OrderBy(e => e.Name)
            .ToListAsync(ct);

        var versions = await _db.ExamVersions
            .AsNoTracking()
            .Where(v => exams.Select(e => e.Id).Contains(v.ExamId))
            .ToListAsync(ct);

        var versionIds = versions.Select(v => v.Id).ToList();

        var attempts = await _db.Attempts
            .AsNoTracking()
            .Where(a => versionIds.Contains(a.ExamVersionId))
            .ToListAsync(ct);

        return exams.Select(e =>
        {
            var examVersionIds = versions.Where(v => v.ExamId == e.Id).Select(v => v.Id).ToHashSet();
            var examAttempts = attempts.Where(a => examVersionIds.Contains(a.ExamVersionId)).ToList();

            var avg = examAttempts.Count == 0 ? 0 : examAttempts.Average(a => a.ScorePercent);
            var last = examAttempts.Count == 0 ? null : examAttempts.Max(a => a.FinishedAt ?? a.StartedAt);

            return new ExamDashboardItemDto(e.Id, e.Name, examAttempts.Count, avg, last);
        }).ToList();
    }
}
