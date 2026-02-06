namespace Quizzer.Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{paramName} no puede ser vací­o", paramName);
        return value;
    }
}
