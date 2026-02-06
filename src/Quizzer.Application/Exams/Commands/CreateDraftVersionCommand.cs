using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Exams.Commands;

public sealed record CreateDraftVersionCommand(Guid ExamId) : IRequest<Guid>;

public sealed class CreateDraftVersionCommandHandler(IQuizzerDbContext db) : IRequestHandler<CreateDraftVersionCommand, Guid>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Guid> Handle(CreateDraftVersionCommand request, CancellationToken ct)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        var existingDraft = await _db.ExamVersions.FirstOrDefaultAsync(v => v.ExamId == exam.Id && v.Status == VersionStatus.Draft, ct);
        if (existingDraft is not null) return existingDraft.Id;

        var latestPublished = await _db.ExamVersions
            .Where(v => v.ExamId == exam.Id && v.Status == VersionStatus.Published)
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync(ct);

        var nextVersionNumber = (latestPublished?.VersionNumber ?? 0) + 1;

        var draft = new ExamVersion
        {
            ExamId = exam.Id,
            VersionNumber = nextVersionNumber,
            Status = VersionStatus.Draft,
            Notes = "",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _db.ExamVersions.Add(draft);
        await _db.SaveChangesAsync(ct);

        // (Opcional para ya soportar versionado real) clonar preguntas del último published
        if (latestPublished is not null)
        {
            var publishedQuestions = await _db.Questions
                .Include(q => q.Options)
                .Where(q => q.ExamVersionId == latestPublished.Id)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync(ct);

            foreach (var pq in publishedQuestions)
            {
                var optionMap = new Dictionary<Guid, Guid>();

                var nq = new Question
                {
                    ExamVersionId = draft.Id,
                    QuestionKey = pq.QuestionKey,
                    Text = pq.Text,
                    Explanation = pq.Explanation,
                    OrderIndex = pq.OrderIndex,
                    Difficulty = pq.Difficulty
                };

                _db.Questions.Add(nq);
                await _db.SaveChangesAsync(ct);

                foreach (var po in pq.Options.OrderBy(o => o.OrderIndex))
                {
                    var no = new Option
                    {
                        QuestionId = nq.Id,
                        OptionKey = po.OptionKey,
                        Text = po.Text,
                        OrderIndex = po.OrderIndex
                    };
                    _db.Options.Add(no);
                    await _db.SaveChangesAsync(ct);
                    optionMap[po.Id] = no.Id;
                }

                nq.CorrectOptionId = optionMap[pq.CorrectOptionId];
                await _db.SaveChangesAsync(ct);
            }
        }

        return draft.Id;
    }
}
