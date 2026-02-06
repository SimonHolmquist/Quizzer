namespace Quizzer.Application.Study;

public sealed class StudySettings
{
    public int InitialIntervalDays { get; init; } = 1;
    public int MinIntervalDays { get; init; } = 1;
    public int MaxIntervalDays { get; init; } = 60;

    public double InitialEaseFactor { get; init; } = 2.5;
    public double MinEaseFactor { get; init; } = 1.3;
    public double MaxEaseFactor { get; init; } = 3.0;
    public double EaseFactorIncrement { get; init; } = 0.05;
    public double EaseFactorDecrement { get; init; } = 0.2;

    public static StudySettings Default { get; } = new();
}
