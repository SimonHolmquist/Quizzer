using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Application.ImportExport.Csv;
using Quizzer.Domain;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Exams.Commands;

public sealed record ImportDraftFromCsvCommand(Guid ExamId, string CsvPath) : IRequest<Unit>;

public sealed class ImportDraftFromCsvCommandHandler(IQuizzerDbContext db) : IRequestHandler<ImportDraftFromCsvCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(ImportDraftFromCsvCommand request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.CsvPath))
            throw new InvalidOperationException("Ruta de CSV inválida.");

        var exam = await _db.Exams.FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        var draft = await _db.ExamVersions
            .FirstOrDefaultAsync(v => v.ExamId == exam.Id && v.Status == VersionStatus.Draft, ct);

        if (draft is null)
        {
            var latestPublished = await _db.ExamVersions
                .Where(v => v.ExamId == exam.Id && v.Status == VersionStatus.Published)
                .OrderByDescending(v => v.VersionNumber)
                .FirstOrDefaultAsync(ct);

            var nextVersionNumber = (latestPublished?.VersionNumber ?? 0) + 1;

            draft = new ExamVersion
            {
                ExamId = exam.Id,
                VersionNumber = nextVersionNumber,
                Status = VersionStatus.Draft,
                Notes = "",
                CreatedAt = DateTimeOffset.UtcNow
            };

            _db.ExamVersions.Add(draft);
            await _db.SaveChangesAsync(ct);
        }

        var (items, errors) = CsvExamImporter.Parse(request.CsvPath);
        if (errors.Count > 0)
            throw new InvalidOperationException(string.Join(Environment.NewLine, errors));

        // Validación mínima: 2+ opciones y 1 correcta
        foreach (var item in items)
        {
            if (string.IsNullOrWhiteSpace(item.Question))
                throw new InvalidOperationException("Pregunta vacía.");
            if (item.Options.Count < 2)
                throw new InvalidOperationException("Cada pregunta requiere 2+ opciones.");
            if (item.CorrectIndex < 0 || item.CorrectIndex >= item.Options.Count)
                throw new InvalidOperationException("Cada pregunta requiere exactamente 1 correcta.");
        }

        var existing = await _db.Questions.Include(q => q.Options)
            .Where(q => q.ExamVersionId == draft.Id)
            .ToListAsync(ct);

        if (existing.Count > 0)
        {
            _db.Options.RemoveRange(existing.SelectMany(q => q.Options));
            _db.Questions.RemoveRange(existing);
            await _db.SaveChangesAsync(ct);
        }

        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var question = new Question
            {
                ExamVersionId = draft.Id,
                QuestionKey = Guid.NewGuid(),
                Text = item.Question.Trim(),
                Explanation = item.Explanation,
                OrderIndex = index,
                Difficulty = item.Difficulty
            };

            _db.Questions.Add(question);
            await _db.SaveChangesAsync(ct);

            var optionIdByIndex = new Dictionary<int, Guid>();

            for (var optionIndex = 0; optionIndex < item.Options.Count; optionIndex++)
            {
                var optionText = item.Options[optionIndex];
                var option = new Option
                {
                    QuestionId = question.Id,
                    OptionKey = Guid.NewGuid(),
                    Text = optionText.Trim(),
                    OrderIndex = optionIndex
                };

                _db.Options.Add(option);
                await _db.SaveChangesAsync(ct);

                optionIdByIndex[optionIndex] = option.Id;
            }

            question.CorrectOptionId = optionIdByIndex[item.CorrectIndex];
            await _db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
