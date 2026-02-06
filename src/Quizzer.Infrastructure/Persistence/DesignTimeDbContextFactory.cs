using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Quizzer.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuizzerDbContext>
{
    public QuizzerDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<QuizzerDbContext>()
            .UseSqlite("Data Source=quizzer.db")
            .Options;

        return new QuizzerDbContext(opts);
    }
}
