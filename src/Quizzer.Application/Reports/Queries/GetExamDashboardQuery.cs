using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetExamDashboardQuery(Guid? ExamId = null, Guid? ExamVersionId = null) : IRequest<ExamDashboardDto>;

public sealed record ExamDashboardDto(
    int TotalAttempts,
    double AverageScorePercent,
    double AverageDurationSeconds,
    DateTimeOffset? LastAttemptAt,
    int TotalQuestions,
    int DueQuestions,
    int WeakQuestions);

public sealed class GetExamDashboardQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetExamDashboardQuery, ExamDashboardDto>
{
    private const int MinAttemptsForWeak = 3;
    private const double WeakAccuracyThreshold = 0.6;

    private readonly IQuizzerDbContext _db = db;

    public async Task<ExamDashboardDto> Handle(GetExamDashboardQuery request, CancellationToken ct)
    {
        var versionIdsQuery = _db.ExamVersions.AsNoTracking()
            .Where(v => (request.ExamId == null || v.ExamId == request.ExamId)
                        && (request.ExamVersionId == null || v.Id == request.ExamVersionId))
            .Select(v => v.Id);

        var attemptsQuery = _db.Attempts.AsNoTracking()
            .Where(a => versionIdsQuery.Contains(a.ExamVersionId));

        var attemptAgg = await attemptsQuery
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                AvgScore = g.Average(a => (double?)a.ScorePercent) ?? 0,
                AvgDuration = g.Average(a => (double?)a.DurationSeconds) ?? 0,
                LastAttemptAt = g.Max(a => (DateTimeOffset?)(a.FinishedAt ?? a.StartedAt))
            })
            .FirstOrDefaultAsync(ct);

        var questionKeysQuery = _db.Questions.AsNoTracking()
            .Where(q => versionIdsQuery.Contains(q.ExamVersionId))
            .Select(q => q.QuestionKey)
            .Distinct();

        var totalQuestions = await questionKeysQuery.CountAsync(ct);

        var now = DateTimeOffset.UtcNow;
        var statsQuery = _db.QuestionStats.AsNoTracking()
            .Where(s => questionKeysQuery.Contains(s.QuestionKey));

        var dueQuestions = await statsQuery
            .Where(s => s.DueAt != null && s.DueAt <= now)
            .CountAsync(ct);

        var weakQuestions = await statsQuery
            .Where(s => s.CorrectCount + s.WrongCount >= MinAttemptsForWeak)
            .Where(s => s.CorrectCount * 1.0 / (s.CorrectCount + s.WrongCount) <= WeakAccuracyThreshold)
            .CountAsync(ct);

        return new ExamDashboardDto(
            attemptAgg?.Total ?? 0,
            attemptAgg?.AvgScore ?? 0,
            attemptAgg?.AvgDuration ?? 0,
            attemptAgg?.LastAttemptAt,
            totalQuestions,
            dueQuestions,
            weakQuestions);
    }
}
