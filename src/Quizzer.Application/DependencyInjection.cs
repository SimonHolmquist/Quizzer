using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Quizzer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        services.AddSingleton(Study.StudySettings.Default);
        services.AddTransient<Study.SpacedRepetitionUpdater>();
        return services;
    }
}
