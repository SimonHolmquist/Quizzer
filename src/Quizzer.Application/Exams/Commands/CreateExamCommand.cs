using MediatR;
using Quizzer.Application.Abstractions;
using Quizzer.Domain.Exams;

namespace Quizzer.Application.Exams.Commands;

public sealed record CreateExamCommand(string Name) : IRequest<Guid>;

public sealed class CreateExamCommandHandler(IQuizzerDbContext db) : IRequestHandler<CreateExamCommand, Guid>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Guid> Handle(CreateExamCommand request, CancellationToken ct)
    {
        var exam = new Exam
        {
            Name = request.Name.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            IsDeleted = false
        };

        _db.Exams.Add(exam);
        await _db.SaveChangesAsync(ct);

        return exam.Id;
    }
}
