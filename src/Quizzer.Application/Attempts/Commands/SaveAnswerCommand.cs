using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Attempts.Commands;

public sealed record SaveAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    Guid SelectedOptionId,
    int SecondsSpent,
    bool FlaggedDoubt) : IRequest<Unit>;

public sealed class SaveAnswerCommandHandler(IQuizzerDbContext db) : IRequestHandler<SaveAnswerCommand, Unit>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task<Unit> Handle(SaveAnswerCommand request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Attempt no encontrado.");

        if (attempt.FinishedAt is not null)
            throw new InvalidOperationException("Attempt ya finalizado.");

        var question = await _db.Questions.AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId && q.ExamVersionId == attempt.ExamVersionId, ct)
            ?? throw new InvalidOperationException("Pregunta no encontrada para esta versión.");

        var option = await _db.Options.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.SelectedOptionId && o.QuestionId == question.Id, ct)
            ?? throw new InvalidOperationException("Opción inválida.");

        var answer = await _db.AttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == attempt.Id && a.QuestionId == question.Id, ct);

        if (answer is null)
        {
            answer = new AttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = question.Id,
                QuestionKey = question.QuestionKey
            };

            _db.AttemptAnswers.Add(answer);
        }

        answer.SelectedOptionId = option.Id;
        answer.SelectedOptionKey = option.OptionKey;
        answer.IsCorrect = option.Id == question.CorrectOptionId;
        answer.SecondsSpent = request.SecondsSpent;
        answer.FlaggedDoubt = request.FlaggedDoubt;
        answer.AnsweredAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);
        return Unit.Value;
    }
}
