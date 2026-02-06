using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Attempts.Commands;

public sealed record FinishAttemptCommand(Guid AttemptId) : IRequest<AttemptSummaryDto>;

public sealed record AttemptSummaryDto(
    Guid AttemptId,
    Guid ExamVersionId,
    int TotalCount,
    int CorrectCount,
    double ScorePercent,
    int DurationSeconds,
    DateTimeOffset? FinishedAt);

public sealed class FinishAttemptCommandHandler(IQuizzerDbContext db) : IRequestHandler<FinishAttemptCommand, AttemptSummaryDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptSummaryDto> Handle(FinishAttemptCommand request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .Include(a => a.Answers)
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Attempt no encontrado.");

        if (attempt.FinishedAt is not null)
            throw new InvalidOperationException("Attempt ya finalizado.");

        var totalCount = await _db.Questions.AsNoTracking()
            .CountAsync(q => q.ExamVersionId == attempt.ExamVersionId, ct);

        var correctCount = attempt.Answers.Count(a => a.IsCorrect);
        attempt.TotalCount = totalCount;
        attempt.CorrectCount = correctCount;
        attempt.ScorePercent = totalCount == 0 ? 0 : correctCount / (double)totalCount * 100.0;
        attempt.FinishedAt = DateTimeOffset.UtcNow;
        attempt.DurationSeconds = (int)Math.Round((attempt.FinishedAt.Value - attempt.StartedAt).TotalSeconds);

        await _db.SaveChangesAsync(ct);

        return new AttemptSummaryDto(
            attempt.Id,
            attempt.ExamVersionId,
            attempt.TotalCount,
            attempt.CorrectCount,
            attempt.ScorePercent,
            attempt.DurationSeconds,
            attempt.FinishedAt);
    }
}
