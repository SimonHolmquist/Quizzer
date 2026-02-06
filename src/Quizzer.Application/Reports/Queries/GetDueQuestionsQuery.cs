using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetDueQuestionsQuery(
    Guid? ExamId = null,
    Guid? ExamVersionId = null,
    DateTimeOffset? AsOf = null) : IRequest<IReadOnlyList<DueQuestionDto>>;

public sealed record DueQuestionDto(
    Guid QuestionKey,
    Guid QuestionId,
    string Text,
    int? Difficulty,
    int TotalAttempts,
    int CorrectCount,
    int WrongCount,
    DateTimeOffset? DueAt,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? LastAnsweredAt);

public sealed class GetDueQuestionsQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetDueQuestionsQuery, IReadOnlyList<DueQuestionDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<DueQuestionDto>> Handle(GetDueQuestionsQuery request, CancellationToken ct)
    {
        var asOf = request.AsOf ?? DateTimeOffset.UtcNow;

        var versionIdsQuery = _db.ExamVersions.AsNoTracking()
            .Where(v => (request.ExamId == null || v.ExamId == request.ExamId)
                        && (request.ExamVersionId == null || v.Id == request.ExamVersionId))
            .Select(v => v.Id);

        var questions = await _db.Questions.AsNoTracking()
            .Where(q => versionIdsQuery.Contains(q.ExamVersionId))
            .Select(q => new { q.QuestionKey, q.Id, q.Text, q.Difficulty })
            .ToListAsync(ct);

        var questionMap = questions
            .GroupBy(q => q.QuestionKey)
            .Select(g => g.First())
            .ToDictionary(q => q.QuestionKey);

        if (questionMap.Count == 0)
            return [];

        var stats = await _db.QuestionStats.AsNoTracking()
            .Where(s => questionMap.Keys.Contains(s.QuestionKey))
            .Where(s => s.DueAt != null && s.DueAt <= asOf)
            .OrderBy(s => s.DueAt)
            .ToListAsync(ct);

        var attemptAgg = await (
            from answer in _db.AttemptAnswers.AsNoTracking()
            join attempt in _db.Attempts.AsNoTracking() on answer.AttemptId equals attempt.Id
            where versionIdsQuery.Contains(attempt.ExamVersionId)
            group answer by answer.QuestionKey
            into g
            select new
            {
                QuestionKey = g.Key,
                TotalAttempts = g.Count(),
                LastAnsweredAt = g.Max(a => (DateTimeOffset?)a.AnsweredAt)
            })
            .ToListAsync(ct);

        var attemptMap = attemptAgg.ToDictionary(x => x.QuestionKey);

        var due = stats.Select(s =>
        {
            var question = questionMap[s.QuestionKey];
            attemptMap.TryGetValue(s.QuestionKey, out var attemptInfo);
            return new DueQuestionDto(
                s.QuestionKey,
                question.Id,
                question.Text,
                question.Difficulty,
                attemptInfo?.TotalAttempts ?? (s.CorrectCount + s.WrongCount),
                s.CorrectCount,
                s.WrongCount,
                s.DueAt,
                s.LastSeenAt,
                attemptInfo?.LastAnsweredAt);
        }).ToList();

        return due;
    }
}
