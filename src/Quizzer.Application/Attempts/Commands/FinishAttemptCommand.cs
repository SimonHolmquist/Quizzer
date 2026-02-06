using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Attempts.Commands;

public sealed record FinishAttemptCommand(Guid AttemptId) : IRequest<AttemptResultDto>;

public sealed record AttemptResultDto(
    Guid AttemptId,
    int TotalCount,
    int CorrectCount,
    double ScorePercent,
    int DurationSeconds);

public sealed class FinishAttemptCommandHandler(IQuizzerDbContext db) : IRequestHandler<FinishAttemptCommand, AttemptResultDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptResultDto> Handle(FinishAttemptCommand request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Intento no encontrado.");

        var answers = await _db.AttemptAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync(ct);

        var correctCount = answers.Count(a => a.IsCorrect);
        var total = attempt.TotalCount == 0 ? answers.Count : attempt.TotalCount;

        attempt.CorrectCount = correctCount;
        attempt.TotalCount = total;
        attempt.ScorePercent = total == 0 ? 0 : correctCount * 100.0 / total;
        attempt.DurationSeconds = (int)(DateTimeOffset.UtcNow - attempt.StartedAt).TotalSeconds;
        attempt.FinishedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new AttemptResultDto(attempt.Id, total, correctCount, attempt.ScorePercent, attempt.DurationSeconds);
    }
}
