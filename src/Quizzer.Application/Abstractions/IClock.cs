namespace Quizzer.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
