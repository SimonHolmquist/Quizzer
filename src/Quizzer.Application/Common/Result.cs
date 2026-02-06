namespace Quizzer.Application.Common;

public readonly record struct Result(bool Success, string? Error = null)
{
    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
}
