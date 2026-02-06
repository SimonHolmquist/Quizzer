using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Exams.Commands;

public sealed record RestoreExamCommand(Guid ExamId) : IRequest<Unit>;

public sealed class RestoreExamCommandHandler(IQuizzerDbContext db) : IRequestHandler<RestoreExamCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(RestoreExamCommand request, CancellationToken ct)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(x => x.Id == request.ExamId, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        if (exam.IsDeleted)
        {
            exam.IsDeleted = false;
            await _db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
