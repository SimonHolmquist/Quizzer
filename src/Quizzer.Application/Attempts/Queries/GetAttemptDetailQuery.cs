using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Attempts.Queries;

public sealed record GetAttemptDetailQuery(Guid AttemptId) : IRequest<AttemptDetailDto>;

public sealed record AttemptDetailDto(
    Guid AttemptId,
    Guid ExamId,
    string ExamName,
    Guid ExamVersionId,
    int VersionNumber,
    DateTimeOffset StartedAt,
    DateTimeOffset? FinishedAt,
    int TotalCount,
    int CorrectCount,
    double ScorePercent,
    int DurationSeconds,
    IReadOnlyList<AttemptQuestionDetailDto> Questions);

public sealed record AttemptQuestionDetailDto(
    Guid QuestionId,
    Guid QuestionKey,
    string Text,
    IReadOnlyList<AttemptOptionDetailDto> Options,
    Guid? SelectedOptionId,
    Guid? SelectedOptionKey,
    bool IsCorrect,
    int? SecondsSpent,
    bool FlaggedDoubt);

public sealed record AttemptOptionDetailDto(
    Guid OptionId,
    Guid OptionKey,
    string Text,
    int OrderIndex,
    bool IsCorrectOption,
    bool IsSelected);

public sealed class GetAttemptDetailQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetAttemptDetailQuery, AttemptDetailDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptDetailDto> Handle(GetAttemptDetailQuery request, CancellationToken ct)
    {
        var attempt = await _db.Attempts.AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Attempt no encontrado.");

        var version = await _db.ExamVersions.AsNoTracking()
            .Include(v => v.Exam)
            .FirstOrDefaultAsync(v => v.Id == attempt.ExamVersionId, ct)
            ?? throw new InvalidOperationException("Versión no encontrada.");

        var questions = await _db.Questions.AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.ExamVersionId == version.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        var answers = await _db.AttemptAnswers.AsNoTracking()
            .Where(a => a.AttemptId == attempt.Id)
            .ToListAsync(ct);

        var answerMap = answers.ToDictionary(a => a.QuestionId, a => a);

        var dtoQuestions = questions.Select(q =>
        {
            answerMap.TryGetValue(q.Id, out var answer);

            var opts = q.Options
                .OrderBy(o => o.OrderIndex)
                .Select(o => new AttemptOptionDetailDto(
                    o.Id,
                    o.OptionKey,
                    o.Text,
                    o.OrderIndex,
                    o.Id == q.CorrectOptionId,
                    answer?.SelectedOptionId == o.Id))
                .ToList();

            return new AttemptQuestionDetailDto(
                q.Id,
                q.QuestionKey,
                q.Text,
                opts,
                answer?.SelectedOptionId,
                answer?.SelectedOptionKey,
                answer?.IsCorrect ?? false,
                answer?.SecondsSpent,
                answer?.FlaggedDoubt ?? false);
        }).ToList();

        return new AttemptDetailDto(
            attempt.Id,
            version.ExamId,
            version.Exam?.Name ?? "",
            version.Id,
            version.VersionNumber,
            attempt.StartedAt,
            attempt.FinishedAt,
            attempt.TotalCount,
            attempt.CorrectCount,
            attempt.ScorePercent,
            attempt.DurationSeconds,
            dtoQuestions);
    }
}
