using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Application.Exams.Queries;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Exams.Commands;

public sealed record UpsertDraftContentCommand(Guid DraftVersionId, IReadOnlyList<ExamQuestionDto> Questions) : IRequest<Unit>;

public sealed class UpsertDraftContentCommandHandler(IQuizzerDbContext db) : IRequestHandler<UpsertDraftContentCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(UpsertDraftContentCommand request, CancellationToken ct)
    {
        var version = await _db.ExamVersions.FirstOrDefaultAsync(v => v.Id == request.DraftVersionId, ct)
            ?? throw new InvalidOperationException("Draft no encontrado.");

        if (version.Status != VersionStatus.Draft)
            throw new InvalidOperationException("Solo se puede editar un Draft.");

        // Validación mínima: 2+ opciones y 1 correcta
        foreach (var q in request.Questions)
        {
            if (string.IsNullOrWhiteSpace(q.Text)) throw new InvalidOperationException("Pregunta vacía.");
            if (q.Options.Count < 2) throw new InvalidOperationException("Cada pregunta requiere 2+ opciones.");
            if (q.Options.Count(o => o.IsCorrect) != 1) throw new InvalidOperationException("Cada pregunta requiere exactamente 1 correcta.");
        }

        // Borramos snapshot del draft y lo recreamos (simple, sólido para v1)
        var existing = await _db.Questions.Include(q => q.Options)
            .Where(q => q.ExamVersionId == version.Id)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _db.Options.RemoveRange(existing.SelectMany(q => q.Options));
            _db.Questions.RemoveRange(existing);
            await _db.SaveChangesAsync(ct);
        }

        foreach (var q in request.Questions.OrderBy(x => x.OrderIndex))
        {
            var nq = new Question
            {
                ExamVersionId = version.Id,
                QuestionKey = q.QuestionKey,
                Text = q.Text.Trim(),
                OrderIndex = q.OrderIndex
            };

            _db.Questions.Add(nq);
            await _db.SaveChangesAsync(ct);

            var optionIdByKey = new Dictionary<Guid, Guid>();

            foreach (var o in q.Options.OrderBy(x => x.OrderIndex))
            {
                var no = new Option
                {
                    QuestionId = nq.Id,
                    OptionKey = o.OptionKey,
                    Text = o.Text.Trim(),
                    OrderIndex = o.OrderIndex
                };

                _db.Options.Add(no);
                await _db.SaveChangesAsync(ct);

                optionIdByKey[o.OptionKey] = no.Id;
            }

            var correctKey = q.Options.Single(x => x.IsCorrect).OptionKey;
            nq.CorrectOptionId = optionIdByKey[correctKey];

            await _db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
