using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Quizzer.Application;
using Quizzer.Infrastructure;
using Quizzer.Infrastructure.Persistence;
using Quizzer.Desktop.Navigation;
using Quizzer.Desktop.ViewModels;
using Quizzer.Desktop.ViewModels.Exams;
using Quizzer.Desktop.ViewModels.Editor;

namespace Quizzer.Desktop;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override async void OnStartup(StartupEventArgs e)
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddApplication();
                services.AddInfrastructure();

                services.AddSingleton<NavigationService>();
                services.AddSingleton<INavigationService>(sp => sp.GetRequiredService<NavigationService>());

                services.AddSingleton<MainWindowViewModel>();
                services.AddSingleton<ExamsListViewModel>();
                services.AddTransient<ExamEditorViewModel>();

                services.AddSingleton(sp =>
                {
                    var w = new MainWindow
                    {
                        DataContext = sp.GetRequiredService<MainWindowViewModel>()
                    };
                    return w;
                });
            })
            .Build();

        await _host.StartAsync();

        await MigrateDatabaseAsync(_host.Services);

        var nav = _host.Services.GetRequiredService<INavigationService>();
        await nav.NavigateToAsync<ExamsListViewModel>();

        _host.Services.GetRequiredService<MainWindow>().Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
        base.OnExit(e);
    }

    private static async Task MigrateDatabaseAsync(IServiceProvider sp)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<QuizzerDbContext>();
        await db.Database.MigrateAsync();
    }
}
