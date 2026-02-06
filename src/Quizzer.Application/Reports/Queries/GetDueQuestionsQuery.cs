using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Reports.Queries;

public sealed record GetDueQuestionsQuery(Guid ExamId, int DaysThreshold = 7) : IRequest<IReadOnlyList<DueQuestionDto>>;

public sealed record DueQuestionDto(
    Guid QuestionId,
    string QuestionText,
    DateTimeOffset? LastAnsweredAt);

public sealed class GetDueQuestionsQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetDueQuestionsQuery, IReadOnlyList<DueQuestionDto>>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<IReadOnlyList<DueQuestionDto>> Handle(GetDueQuestionsQuery request, CancellationToken ct)
    {
        var latestVersion = await _db.ExamVersions
            .AsNoTracking()
            .Where(v => v.ExamId == request.ExamId && v.Status == VersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);

        if (latestVersion is null)
            return [];

        var questions = await _db.Questions
            .AsNoTracking()
            .Where(q => q.ExamVersionId == latestVersion.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        if (questions.Count == 0)
            return [];

        var questionIds = questions.Select(q => q.Id).ToList();
        var answers = await _db.AttemptAnswers
            .AsNoTracking()
            .Where(a => questionIds.Contains(a.QuestionId))
            .ToListAsync(ct);

        var lastAnswered = answers
            .GroupBy(a => a.QuestionId)
            .ToDictionary(g => g.Key, g => g.Max(a => a.AnsweredAt));

        var threshold = DateTimeOffset.UtcNow.AddDays(-request.DaysThreshold);

        return questions
            .Select(q => new DueQuestionDto(
                q.Id,
                q.Text,
                lastAnswered.TryGetValue(q.Id, out var last) ? last : null))
            .Where(dto => dto.LastAnsweredAt is null || dto.LastAnsweredAt < threshold)
            .ToList();
    }
}
