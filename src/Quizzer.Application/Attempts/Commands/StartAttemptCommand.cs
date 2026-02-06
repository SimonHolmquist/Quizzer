using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Attempts.Commands;

public sealed record StartAttemptCommand(Guid ExamVersionId) : IRequest<AttemptSessionDto>;

public sealed record AttemptSessionDto(
    Guid AttemptId,
    Guid ExamId,
    string ExamName,
    int VersionNumber,
    IReadOnlyList<AttemptQuestionDto> Questions);

public sealed record AttemptQuestionDto(
    Guid QuestionId,
    Guid QuestionKey,
    string Text,
    IReadOnlyList<AttemptOptionDto> Options);

public sealed record AttemptOptionDto(
    Guid OptionId,
    Guid OptionKey,
    string Text);

public sealed class StartAttemptCommandHandler(IQuizzerDbContext db) : IRequestHandler<StartAttemptCommand, AttemptSessionDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptSessionDto> Handle(StartAttemptCommand request, CancellationToken ct)
    {
        var version = await _db.ExamVersions
            .Include(v => v.Exam)
            .FirstOrDefaultAsync(v => v.Id == request.ExamVersionId, ct)
            ?? throw new InvalidOperationException("Versión no encontrada.");

        var questions = await _db.Questions
            .Include(q => q.Options)
            .Where(q => q.ExamVersionId == version.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        var attempt = new Attempt
        {
            ExamVersionId = version.Id,
            StartedAt = DateTimeOffset.UtcNow,
            TotalCount = questions.Count
        };

        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        var dtoQuestions = questions.Select(q => new AttemptQuestionDto(
            q.Id,
            q.QuestionKey,
            q.Text,
            q.Options.OrderBy(o => o.OrderIndex)
                .Select(o => new AttemptOptionDto(o.Id, o.OptionKey, o.Text))
                .ToList()
        )).ToList();

        return new AttemptSessionDto(
            attempt.Id,
            version.ExamId,
            version.Exam?.Name ?? "",
            version.VersionNumber,
            dtoQuestions);
    }
}
