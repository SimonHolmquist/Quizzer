using Shouldly;
using Quizzer.Domain.Study;
using Xunit;

namespace Quizzer.Tests.Domain;

public sealed class SpacedRepetitionTests
{
    [Fact]
    public void WrongAnswer_SetsIntervalToOneDay()
    {
        var s = new QuestionStats { QuestionKey = Guid.NewGuid(), IntervalDays = 10, EaseFactor = 2.5 };
        SpacedRepetition.ApplyResult(s, correct: false, now: DateTimeOffset.UtcNow);
        s.IntervalDays.ShouldBe(1);
    }
}
