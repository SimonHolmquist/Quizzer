using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Exams.Commands;

public sealed record PublishVersionCommand(Guid DraftVersionId, string Notes) : IRequest<Unit>;

public sealed class PublishVersionCommandHandler(IQuizzerDbContext db) : IRequestHandler<PublishVersionCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(PublishVersionCommand request, CancellationToken ct)
    {
        var version = await _db.ExamVersions.FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, ct)
            ?? throw new InvalidOperationException("Versión no encontrada.");

        if (version.Status != VersionStatus.Draft)
            throw new InvalidOperationException("Solo se puede publicar un Draft.");

        if (string.IsNullOrWhiteSpace(request.Notes))
            throw new InvalidOperationException("Notas de versión obligatorias.");

        var questions = await _db.Questions.Include(q => q.Options)
            .Where(q => q.ExamVersionId == version.Id)
            .ToListAsync(ct);

        if (questions.Count == 0)
            throw new InvalidOperationException("No se puede publicar una versión sin preguntas.");

        foreach (var q in questions)
        {
            if (q.Options.Count < 2) throw new InvalidOperationException("Pregunta con menos de 2 opciones.");
            if (!q.Options.Any(o => o.Id == q.CorrectOptionId)) throw new InvalidOperationException("CorrectOption inválida.");
        }

        version.Notes = request.Notes.Trim();
        version.Status = VersionStatus.Published;
        version.PublishedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
