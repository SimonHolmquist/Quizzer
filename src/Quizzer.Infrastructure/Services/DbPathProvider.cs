namespace Quizzer.Infrastructure.Services;

public sealed class DbPathProvider
{
    public static string DbPath
    {
        get
        {
            // TODO: definir ubicación definitiva (AppData/Local/Quizzer/quizzer.db)
            var baseDir = AppContext.BaseDirectory;
            return Path.Combine(baseDir, "quizzer.db");
        }
    }
}
