using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Quizzer.Infrastructure.Services;

namespace Quizzer.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuizzerDbContext>
{
    public QuizzerDbContext CreateDbContext(string[] args)
    {
        var dbPath = DbPathProvider.GetDbPath();
        var opts = new DbContextOptionsBuilder<QuizzerDbContext>()
            .UseSqlite($"Data Source={dbPath}")
            .Options;

        return new QuizzerDbContext(opts);
    }
}
