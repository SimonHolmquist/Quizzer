using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Quizzer.Application.Abstractions;
using Quizzer.Infrastructure.Services;
using Quizzer.Infrastructure.Persistence;

namespace Quizzer.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddSingleton<DbPathProvider>();

        services.AddDbContext<QuizzerDbContext>(static (sp, opts) =>
        {
            var dbPath = DbPathProvider.DbPath;
            opts.UseSqlite($"Data Source={dbPath}");
        });

        services.AddScoped<IQuizzerDbContext, QuizzerDbContext>();

        return services;
    }
}
