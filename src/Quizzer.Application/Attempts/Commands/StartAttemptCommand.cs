using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Attempts.Commands;

public sealed record StartAttemptCommand(Guid ExamId) : IRequest<AttemptStartDto>;

public sealed record AttemptStartDto(
    Guid AttemptId,
    Guid ExamId,
    Guid ExamVersionId,
    int VersionNumber,
    int TotalCount,
    DateTimeOffset StartedAt);

public sealed class StartAttemptCommandHandler(IQuizzerDbContext db) : IRequestHandler<StartAttemptCommand, AttemptStartDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<AttemptStartDto> Handle(StartAttemptCommand request, CancellationToken ct)
    {
        var exam = await _db.Exams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        var version = await _db.ExamVersions.AsNoTracking()
            .Where(v => v.ExamId == exam.Id && v.Status == VersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("No hay versiones publicadas para este examen.");

        var totalCount = await _db.Questions.AsNoTracking()
            .CountAsync(q => q.ExamVersionId == version.Id, ct);

        if (totalCount == 0)
            throw new InvalidOperationException("No se puede iniciar un intento sin preguntas.");

        var attempt = new Attempt
        {
            ExamVersionId = version.Id,
            TotalCount = totalCount
        };

        _db.Attempts.Add(attempt);
        await _db.SaveChangesAsync(ct);

        return new AttemptStartDto(attempt.Id, exam.Id, version.Id, version.VersionNumber, attempt.TotalCount, attempt.StartedAt);
    }
}
