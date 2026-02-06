namespace Quizzer.Infrastructure.Services;

public sealed class DbPathProvider
{
    public static string GetDbPath()
    {
        var baseDir = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var dir = Path.Combine(baseDir, "Quizzer");
        Directory.CreateDirectory(dir);
        return Path.Combine(dir, "quizzer.db");
    }
}
