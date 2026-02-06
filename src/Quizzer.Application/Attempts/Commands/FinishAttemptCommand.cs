using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Application.Common;
using Quizzer.Application.Study;

namespace Quizzer.Application.Attempts.Commands;

public sealed record FinishAttemptCommand(Guid AttemptId) : IRequest<Result>;

public sealed class FinishAttemptCommandHandler : IRequestHandler<FinishAttemptCommand, Result>
{
    private readonly IQuizzerDbContext _db;
    private readonly IClock _clock;
    private readonly SpacedRepetitionUpdater _updater;

    public FinishAttemptCommandHandler(IQuizzerDbContext db, IClock clock, StudySettings settings)
    {
        _db = db;
        _clock = clock;
        _updater = new SpacedRepetitionUpdater(settings ?? StudySettings.Default);
    }

    public async Task<Result> Handle(FinishAttemptCommand request, CancellationToken cancellationToken)
    {
        var attempt = await _db.Attempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, cancellationToken);

        if (attempt is null)
        {
            return Result.Fail(AppErrors.NotFound);
        }

        var now = _clock.UtcNow;
        attempt.FinishedAt = now;
        attempt.TotalCount = attempt.Answers.Count;
        attempt.CorrectCount = attempt.Answers.Count(a => a.IsCorrect);
        attempt.ScorePercent = attempt.TotalCount == 0
            ? 0
            : Math.Round((double)attempt.CorrectCount / attempt.TotalCount * 100, 2);
        attempt.DurationSeconds = (int)Math.Max(0, (now - attempt.StartedAt).TotalSeconds);

        var questionKeys = attempt.Answers
            .Select(a => a.QuestionKey)
            .Distinct()
            .ToList();

        if (questionKeys.Count > 0)
        {
            var stats = await _db.QuestionStats
                .Where(s => questionKeys.Contains(s.QuestionKey))
                .ToListAsync(cancellationToken);

            var statsByKey = stats.ToDictionary(s => s.QuestionKey);
            var created = _updater.ApplyAttempt(attempt.Answers, statsByKey, now);

            if (created.Count > 0)
            {
                _db.QuestionStats.AddRange(created);
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }
}
