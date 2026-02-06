using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Exams.Queries;

public sealed record GetExamDetailQuery(Guid ExamId) : IRequest<ExamDetailDto>;

public sealed record ExamDetailDto(Guid ExamId, string Name, DraftVersionDto? DraftVersion);

public sealed record DraftVersionDto(Guid VersionId, int VersionNumber, IReadOnlyList<ExamQuestionDto> Questions);

public sealed record ExamQuestionDto(Guid QuestionKey, int OrderIndex, string Text, IReadOnlyList<ExamOptionDto> Options);

public sealed record ExamOptionDto(Guid OptionKey, int OrderIndex, string Text, bool IsCorrect);

public sealed class GetExamDetailQueryHandler(IQuizzerDbContext db) : IRequestHandler<GetExamDetailQuery, ExamDetailDto>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<ExamDetailDto> Handle(GetExamDetailQuery request, CancellationToken ct)
    {
        var exam = await _db.Exams.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == request.ExamId && !x.IsDeleted, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        var draft = await _db.ExamVersions.AsNoTracking()
            .Where(v => v.ExamId == exam.Id && v.Status == VersionStatus.Draft)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (draft is null)
            return new ExamDetailDto(exam.Id, exam.Name, null);

        var questions = await _db.Questions.AsNoTracking()
            .Include(q => q.Options)
            .Where(q => q.ExamVersionId == draft.Id)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ct);

        var dtoQuestions = questions.Select(q =>
        {
            var opts = q.Options.OrderBy(o => o.OrderIndex)
                .Select(o => new ExamOptionDto(o.OptionKey, o.OrderIndex, o.Text, o.Id == q.CorrectOptionId))
                .ToList();

            return new ExamQuestionDto(q.QuestionKey, q.OrderIndex, q.Text, opts);
        }).ToList();

        return new ExamDetailDto(exam.Id, exam.Name, new DraftVersionDto(draft.Id, draft.VersionNumber, dtoQuestions));
    }
}
