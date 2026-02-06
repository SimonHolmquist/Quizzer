using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetWeakQuestionsQuery(
    Guid? ExamId = null,
    Guid? ExamVersionId = null,
    int MinAttempts = 3,
    double MaxAccuracy = 0.6) : IRequest<IReadOnlyList<WeakQuestionDto>>;

public sealed record WeakQuestionDto(
    Guid QuestionKey,
    Guid QuestionId,
    string Text,
    int? Difficulty,
    int TotalAttempts,
    int CorrectCount,
    int WrongCount,
    double Accuracy,
    DateTimeOffset? LastSeenAt,
    DateTimeOffset? LastAnsweredAt);

public sealed class GetWeakQuestionsQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetWeakQuestionsQuery, IReadOnlyList<WeakQuestionDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<WeakQuestionDto>> Handle(GetWeakQuestionsQuery request, CancellationToken ct)
    {
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

        var weak = stats
            .Select(s =>
            {
                var total = s.CorrectCount + s.WrongCount;
                var accuracy = total > 0 ? s.CorrectCount * 1.0 / total : 0;
                return new { Stat = s, Total = total, Accuracy = accuracy };
            })
            .Where(x => x.Total >= request.MinAttempts && x.Accuracy <= request.MaxAccuracy)
            .OrderBy(x => x.Accuracy)
            .ThenByDescending(x => x.Stat.WrongCount)
            .Select(x =>
            {
                var question = questionMap[x.Stat.QuestionKey];
                attemptMap.TryGetValue(x.Stat.QuestionKey, out var attemptInfo);
                return new WeakQuestionDto(
                    x.Stat.QuestionKey,
                    question.Id,
                    question.Text,
                    question.Difficulty,
                    attemptInfo?.TotalAttempts ?? x.Total,
                    x.Stat.CorrectCount,
                    x.Stat.WrongCount,
                    x.Accuracy,
                    x.Stat.LastSeenAt,
                    attemptInfo?.LastAnsweredAt);
            })
            .ToList();

        return weak;
    }
}
