using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetWeakQuestionsQuery(Guid ExamId, int Take = 10) : IRequest<IReadOnlyList<WeakQuestionDto>>;

public sealed record WeakQuestionDto(
    Guid QuestionId,
    string QuestionText,
    int TotalAnswers,
    int CorrectAnswers,
    double AccuracyPercent);

public sealed class GetWeakQuestionsQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetWeakQuestionsQuery, IReadOnlyList<WeakQuestionDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<WeakQuestionDto>> Handle(GetWeakQuestionsQuery request, CancellationToken ct)
    {
        var versionIds = await _db.ExamVersions
            .AsNoTracking()
            .Where(v => v.ExamId == request.ExamId)
            .Select(v => v.Id)
            .ToListAsync(ct);

        var attempts = await _db.Attempts
            .AsNoTracking()
            .Where(a => versionIds.Contains(a.ExamVersionId))
            .Select(a => a.Id)
            .ToListAsync(ct);

        var answers = await _db.AttemptAnswers
            .AsNoTracking()
            .Where(a => attempts.Contains(a.AttemptId))
            .ToListAsync(ct);

        if (answers.Count == 0)
            return [];

        var questionIds = answers.Select(a => a.QuestionId).Distinct().ToList();
        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => questionIds.Contains(q.Id))
            .ToDictionaryAsync(q => q.Id, ct);

        var stats = answers.GroupBy(a => a.QuestionId)
            .Select(g =>
            {
                var total = g.Count();
                var correct = g.Count(a => a.IsCorrect);
                var accuracy = total == 0 ? 0 : correct * 100.0 / total;
                return new { g.Key, total, correct, accuracy };
            })
            .OrderBy(s => s.accuracy)
            .ThenByDescending(s => s.total)
            .Take(request.Take)
            .Select(s => new WeakQuestionDto(
                s.Key,
                questions.TryGetValue(s.Key, out var q) ? q.Text : "(pregunta)",
                s.total,
                s.correct,
                s.accuracy))
            .ToList();

        return stats;
    }
}
