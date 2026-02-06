namespace Quizzer.Domain.Study;

public static class SpacedRepetition
{
    public static void ApplyResult(QuestionStats s, bool correct, DateTimeOffset now)
    {
        s.LastSeenAt = now;

        if (correct)
        {
            s.LastCorrectAt = now;
            s.CorrectCount++;

            s.EaseFactor = Math.Min(3.0, s.EaseFactor + 0.05);
            s.IntervalDays = Math.Min(60, Math.Max(1, (int)Math.Round(s.IntervalDays * s.EaseFactor)));
        }
        else
        {
            s.WrongCount++;

            s.EaseFactor = Math.Max(1.3, s.EaseFactor - 0.2);
            s.IntervalDays = 1;
        }

        s.DueAt = now.AddDays(s.IntervalDays);
    }
}
