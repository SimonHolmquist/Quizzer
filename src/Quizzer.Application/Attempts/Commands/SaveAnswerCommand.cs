using MediatR;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application.Abstractions;
using Quizzer.Domain;

namespace Quizzer.Application.Attempts.Commands;

public sealed record SaveAnswerCommand(
    Guid AttemptId,
    Guid QuestionId,
    Guid SelectedOptionId,
    bool FlaggedDoubt,
    int SecondsSpent) : IRequest;

public sealed class SaveAnswerCommandHandler(IQuizzerDbContext db) : IRequestHandler<SaveAnswerCommand>
{
    private readonly IQuizzerDbContext _db = db;

    public async Task Handle(SaveAnswerCommand request, CancellationToken ct)
    {
        var attempt = await _db.Attempts
            .FirstOrDefaultAsync(a => a.Id == request.AttemptId, ct)
            ?? throw new InvalidOperationException("Intento no encontrado.");

        var question = await _db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId, ct)
            ?? throw new InvalidOperationException("Pregunta no encontrada.");

        var option = await _db.Options
            .AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == request.SelectedOptionId && o.QuestionId == question.Id, ct)
            ?? throw new InvalidOperationException("Opción no encontrada.");

        var answer = await _db.AttemptAnswers
            .FirstOrDefaultAsync(a => a.AttemptId == attempt.Id && a.QuestionId == question.Id, ct);

        var isCorrect = option.Id == question.CorrectOptionId;

        if (answer is null)
        {
            answer = new AttemptAnswer
            {
                AttemptId = attempt.Id,
                QuestionId = question.Id,
                QuestionKey = question.QuestionKey,
                SelectedOptionId = option.Id,
                SelectedOptionKey = option.OptionKey,
                IsCorrect = isCorrect,
                AnsweredAt = DateTimeOffset.UtcNow,
                SecondsSpent = request.SecondsSpent,
                FlaggedDoubt = request.FlaggedDoubt
            };

            _db.AttemptAnswers.Add(answer);
        }
        else
        {
            answer.SelectedOptionId = option.Id;
            answer.SelectedOptionKey = option.OptionKey;
            answer.IsCorrect = isCorrect;
            answer.AnsweredAt = DateTimeOffset.UtcNow;
            answer.SecondsSpent = request.SecondsSpent;
            answer.FlaggedDoubt = request.FlaggedDoubt;
        }

        await _db.SaveChangesAsync(ct);
    }
}
