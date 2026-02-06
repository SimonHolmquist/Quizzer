using Quizzer.Domain;
using Quizzer.Domain.Study;

namespace Quizzer.Application.Study;

public sealed class SpacedRepetitionUpdater
{
    private readonly StudySettings _settings;

    public SpacedRepetitionUpdater(StudySettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public QuestionStats CreateStats(Guid questionKey)
        => new()
        {
            QuestionKey = questionKey,
            IntervalDays = _settings.InitialIntervalDays,
            EaseFactor = _settings.InitialEaseFactor
        };

    public void ApplyResult(QuestionStats stats, bool correct, DateTimeOffset now)
    {
        if (stats is null)
        {
            throw new ArgumentNullException(nameof(stats));
        }

        stats.LastSeenAt = now;
        stats.IntervalDays = stats.IntervalDays > 0 ? stats.IntervalDays : _settings.InitialIntervalDays;
        stats.EaseFactor = stats.EaseFactor > 0 ? stats.EaseFactor : _settings.InitialEaseFactor;

        if (correct)
        {
            stats.LastCorrectAt = now;
            stats.CorrectCount++;
            stats.EaseFactor = Math.Min(_settings.MaxEaseFactor, stats.EaseFactor + _settings.EaseFactorIncrement);

            var nextInterval = (int)Math.Round(stats.IntervalDays * stats.EaseFactor);
            stats.IntervalDays = Math.Clamp(nextInterval, _settings.MinIntervalDays, _settings.MaxIntervalDays);
        }
        else
        {
            stats.WrongCount++;
            stats.EaseFactor = Math.Max(_settings.MinEaseFactor, stats.EaseFactor - _settings.EaseFactorDecrement);
            stats.IntervalDays = _settings.MinIntervalDays;
        }

        stats.DueAt = now.AddDays(stats.IntervalDays);
    }

    public IReadOnlyList<QuestionStats> ApplyAttempt(IEnumerable<AttemptAnswer> answers, IDictionary<Guid, QuestionStats> statsByKey, DateTimeOffset now)
    {
        if (answers is null)
        {
            throw new ArgumentNullException(nameof(answers));
        }

        if (statsByKey is null)
        {
            throw new ArgumentNullException(nameof(statsByKey));
        }

        var created = new List<QuestionStats>();

        foreach (var answer in answers)
        {
            if (!statsByKey.TryGetValue(answer.QuestionKey, out var stats))
            {
                stats = CreateStats(answer.QuestionKey);
                statsByKey.Add(answer.QuestionKey, stats);
                created.Add(stats);
            }

            ApplyResult(stats, answer.IsCorrect, now);
        }

        return created;
    }
}
