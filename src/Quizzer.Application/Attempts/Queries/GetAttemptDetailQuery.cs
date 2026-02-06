using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Attempts.Queries;

public sealed record GetAttemptDetailQuery(Guid AttemptId) : IRequest<AttemptDetailDto>;

public sealed record AttemptDetailDto(
    Guid AttemptId,
    string ExamName,
    int VersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    int TotalCount,
    int CorrectCount,
    double ScorePercent,
    IReadOnlyList<AttemptAnswerDto> Answers);

public sealed record AttemptAnswerDto(
    Guid QuestionId,
    string QuestionText,
    Guid SelectedOptionId,
    string SelectedOptionText,
    bool IsCorrect,
    DateTimeOffset AnsweredAt,
    int SecondsSpent,
    bool FlaggedDoubt);

public sealed class GetAttemptDetailQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetAttemptDetailQuery, AttemptDetailDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptDetailDto> Handle(GetAttemptDetailQuery request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Intento no encontrado.");

        var version = await _db.ExamVersions
            .AsNoTracking()
            .Include(v => v.Exam)
            .FirstOrDefaultAsync(v => v.Id == attempt.ExamVersionId, ct)
            ?? throw new InvalidOperationException("Versión no encontrada.");

        var answers = await _db.AttemptAnswers
            .AsNoTracking()
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync(ct);

        var questionIds = answers.Select(a => a.QuestionId).Distinct().ToList();
        var optionIds = answers.Select(a => a.SelectedOptionId).Distinct().ToList();

        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, ct);

        var options = await _db.Options
            .AsNoTracking()
            .Where(o => optionIds.Contains(o.Id))
            .ToDictionaryAsync(o => o.Id, ct);

        var answerDtos = answers
            .OrderBy(a => questions.TryGetValue(a.QuestionId, out var q) ? q.OrderIndex : 0)
            .Select(a => new AttemptAnswerDto(
                a.QuestionId,
                questions.TryGetValue(a.QuestionId, out var q) ? q.Text : "(pregunta)",
                a.SelectedOptionId,
                options.TryGetValue(a.SelectedOptionId, out var o) ? o.Text : "(opción)",
                a.IsCorrect,
                a.AnsweredAt,
                a.SecondsSpent,
                a.FlaggedDoubt))
            .ToList();

        return new AttemptDetailDto(
            attempt.Id,
            version.Exam?.Name ?? "",
            version.VersionNumber,
            attempt.StartedAt,
            attempt.FinishedAt,
            attempt.TotalCount,
            attempt.CorrectCount,
            attempt.ScorePercent,
            answerDtos);
    }
}
