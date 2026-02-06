using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;

namespace Quizzer.Application.Exams.Commands;

public sealed record SoftDeleteExamCommand(Guid ExamId) : IRequest<Unit>;

public sealed class SoftDeleteExamCommandHandler(IQuizzerDbContext db) : IRequestHandler<SoftDeleteExamCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(SoftDeleteExamCommand request, CancellationToken ct)
    {
        var exam = await _db.Exams.FirstOrDefaultAsync(x => x.Id == request.ExamId, ct)
            ?? throw new InvalidOperationException("Exam no encontrado.");

        if (!exam.IsDeleted)
        {
            exam.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }

        return Unit.Value;
    }
}
