# scripts/scaffold-target.ps1
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Ensure-Dir([string]$path) {
  if (-not (Test-Path -LiteralPath $path)) {
    New-Item -ItemType Directory -Force -Path $path | Out-Null
  }
}

function Ensure-File([string]$path, [string]$content) {
  $dir = Split-Path -Parent $path
  if ($dir) { Ensure-Dir $dir }
  if (-not (Test-Path -LiteralPath $path)) {
    Set-Content -LiteralPath $path -Value $content -Encoding UTF8
  }
}

function Move-IfPresent([string]$from, [string]$to) {
  if (Test-Path -LiteralPath $from) {
    $toDir = Split-Path -Parent $to
    Ensure-Dir $toDir
    if (-not (Test-Path -LiteralPath $to)) {
      Move-Item -LiteralPath $from -Destination $to
    }
  }
}

# Debe ejecutarse desde la raiz (donde está Quizzer.sln)
if (-not (Test-Path -LiteralPath "Quizzer.sln")) {
  throw "Ejecutá este script desde la carpeta raiz donde está Quizzer.sln"
}

# 1) Carpetas objetivo
$dirs = @(
  "scripts",
  "src/Quizzer.Domain/Common",
  "src/Quizzer.Domain/Exams",
  "src/Quizzer.Domain/Attempts",
  "src/Quizzer.Domain/Study",

  "src/Quizzer.Application/Abstractions",
  "src/Quizzer.Application/Common",
  "src/Quizzer.Application/ImportExport/Csv",
  "src/Quizzer.Application/Exams/Commands",
  "src/Quizzer.Application/Exams/Queries",
  "src/Quizzer.Application/Attempts/Commands",
  "src/Quizzer.Application/Attempts/Queries",
  "src/Quizzer.Application/Reports/Queries",
  "src/Quizzer.Application/Study",

  "src/Quizzer.Infrastructure/Persistence/Migrations",
  "src/Quizzer.Infrastructure/Persistence/Configurations",
  "src/Quizzer.Infrastructure/Services",

  "src/Quizzer.Desktop/Resources",
  "src/Quizzer.Desktop/Navigation",
  "src/Quizzer.Desktop/Services",
  "src/Quizzer.Desktop/ViewModels/Exams",
  "src/Quizzer.Desktop/ViewModels/Editor",
  "src/Quizzer.Desktop/ViewModels/Attempt",
  "src/Quizzer.Desktop/ViewModels/Reports",
  "src/Quizzer.Desktop/Views/Exams",
  "src/Quizzer.Desktop/Views/Editor",
  "src/Quizzer.Desktop/Views/Attempt",
  "src/Quizzer.Desktop/Views/Reports",

  "tests/Quizzer.Tests/TestAssets",
  "tests/Quizzer.Tests/Domain",
  "tests/Quizzer.Tests/Application"
)

$dirs | ForEach-Object { Ensure-Dir $_ }

# 2) Reubicar archivos existentes (sin cambiar contenido)
Move-IfPresent "src/Quizzer.Domain/Exam.cs"          "src/Quizzer.Domain/Exams/Exam.cs"
Move-IfPresent "src/Quizzer.Domain/ExamVersion.cs"   "src/Quizzer.Domain/Exams/ExamVersion.cs"
Move-IfPresent "src/Quizzer.Domain/Question.cs"      "src/Quizzer.Domain/Exams/Question.cs"
Move-IfPresent "src/Quizzer.Domain/Option.cs"        "src/Quizzer.Domain/Exams/Option.cs"
Move-IfPresent "src/Quizzer.Domain/VersionStatus.cs" "src/Quizzer.Domain/Exams/VersionStatus.cs"
Move-IfPresent "src/Quizzer.Domain/Attempt.cs"       "src/Quizzer.Domain/Attempts/Attempt.cs"
Move-IfPresent "src/Quizzer.Domain/AttemptAnswer.cs" "src/Quizzer.Domain/Attempts/AttemptAnswer.cs"

Move-IfPresent "src/Quizzer.Application/CsvExamImporter.cs" "src/Quizzer.Application/ImportExport/Csv/CsvExamImporter.cs"
Move-IfPresent "src/Quizzer.Application/ImportRow.cs"       "src/Quizzer.Application/ImportExport/Csv/ImportRow.cs"

Move-IfPresent "src/Quizzer.Infrastructure/QuizzerDbContext.cs" "src/Quizzer.Infrastructure/Persistence/QuizzerDbContext.cs"

# 3) Archivos base (root)
Ensure-File ".gitignore" @"
**/bin/
**/obj/
.vs/
.idea/
*.user
*.suo
*.log
TestResults/
coverage/
*.coverage
*.coveragexml
*.trx

# SQLite local
*.db
*.db-shm
*.db-wal
*.sqlite
*.sqlite3

# OS / temp
.DS_Store
Thumbs.db
*.tmp
*.bak
*~
"@

Ensure-File ".editorconfig" @"
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4

[*.{cs,csproj,props,targets,xaml}]
indent_size = 4

[*.md]
trim_trailing_whitespace = false
"@

Ensure-File "README.md" @"
# Quizzer

App Windows local para gestionar exámenes multiple-choice con versionado, editor interno, intentos, reportes y repetición espaciada.

Estado: scaffolding + base de arquitectura (Domain/Application/Infrastructure/Desktop).
"@

# 4) Scripts auxiliares (stubs útiles)
Ensure-File "scripts/ef-add-migration.ps1" @"
param([string]`$Name = ""Init"")
dotnet ef migrations add `$Name --project src/Quizzer.Infrastructure --startup-project src/Quizzer.Desktop
"@

Ensure-File "scripts/ef-update-db.ps1" @"
dotnet ef database update --project src/Quizzer.Infrastructure --startup-project src/Quizzer.Desktop
"@

Ensure-File "scripts/reset-local-db.ps1" @"
# TODO: cuando definamos la ruta real de la db, borrar el archivo aquí.
Write-Host ""Pendiente: definir ruta de SQLite y borrar el archivo .db""
"@

# 5) Domain stubs
Ensure-File "src/Quizzer.Domain/Common/Guard.cs" @"
namespace Quizzer.Domain.Common;

public static class Guard
{
    public static string NotNullOrWhiteSpace(string? value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException(`$""{paramName} no puede ser vacío."", paramName);
        return value;
    }
}
"@

Ensure-File "src/Quizzer.Domain/Common/DomainErrors.cs" @"
namespace Quizzer.Domain.Common;

public static class DomainErrors
{
    public const string InvalidCorrectAnswer = ""La pregunta debe tener exactamente una opción correcta."";
    public const string AtLeastTwoOptions = ""La pregunta debe tener al menos 2 opciones."";
}
"@

Ensure-File "src/Quizzer.Domain/Study/QuestionStats.cs" @"
namespace Quizzer.Domain.Study;

public sealed class QuestionStats
{
    public Guid Id { get; init; } = Guid.NewGuid();

    // Estable por pregunta a través de versiones
    public Guid QuestionKey { get; set; }

    public DateTimeOffset? LastSeenAt { get; set; }
    public DateTimeOffset? LastCorrectAt { get; set; }

    public int CorrectCount { get; set; }
    public int WrongCount { get; set; }

    // Spaced repetition (simplificado)
    public double EaseFactor { get; set; } = 2.5;
    public int IntervalDays { get; set; } = 1;
    public DateTimeOffset? DueAt { get; set; }
}
"@

Ensure-File "src/Quizzer.Domain/Study/SpacedRepetition.cs" @"
namespace Quizzer.Domain.Study;

public static class SpacedRepetition
{
    public static void ApplyResult(QuestionStats s, bool correct, DateTimeOffset now)
    {
        s.LastSeenAt = now;

        if (correct)
        {
            s.LastCorrectAt = now;
            s.CorrectCount++;

            s.EaseFactor = Math.Min(3.0, s.EaseFactor + 0.05);
            s.IntervalDays = Math.Min(60, Math.Max(1, (int)Math.Round(s.IntervalDays * s.EaseFactor)));
        }
        else
        {
            s.WrongCount++;

            s.EaseFactor = Math.Max(1.3, s.EaseFactor - 0.2);
            s.IntervalDays = 1;
        }

        s.DueAt = now.AddDays(s.IntervalDays);
    }
}
"@

# 6) Application stubs
Ensure-File "src/Quizzer.Application/DependencyInjection.cs" @"
using Microsoft.Extensions.DependencyInjection;
using MediatR;

namespace Quizzer.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
"@

Ensure-File "src/Quizzer.Application/Abstractions/IClock.cs" @"
namespace Quizzer.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
"@

Ensure-File "src/Quizzer.Application/Abstractions/IQuizzerDbContext.cs" @"
using Microsoft.EntityFrameworkCore;
using Quizzer.Domain;

namespace Quizzer.Application.Abstractions;

public interface IQuizzerDbContext
{
    DbSet<Exam> Exams { get; }
    DbSet<ExamVersion> ExamVersions { get; }
    DbSet<Question> Questions { get; }
    DbSet<Option> Options { get; }
    DbSet<Attempt> Attempts { get; }
    DbSet<AttemptAnswer> AttemptAnswers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
"@

Ensure-File "src/Quizzer.Application/Common/Result.cs" @"
namespace Quizzer.Application.Common;

public readonly record struct Result(bool Success, string? Error = null)
{
    public static Result Ok() => new(true, null);
    public static Result Fail(string error) => new(false, error);
}
"@

Ensure-File "src/Quizzer.Application/Common/AppErrors.cs" @"
namespace Quizzer.Application.Common;

public static class AppErrors
{
    public const string NotFound = ""No encontrado."";
    public const string ValidationFailed = ""Validación fallida."";
}
"@

Ensure-File "src/Quizzer.Application/ImportExport/Csv/CsvExamExporter.cs" @"
namespace Quizzer.Application.ImportExport.Csv;

public static class CsvExamExporter
{
    // TODO: exportar una versión a CSV (question;option1..N;answer;explanation;tags;difficulty)
}
"@

# Commands/Queries placeholders (vacíos pero presentes)
$placeholders = @(
  "src/Quizzer.Application/Exams/Commands/CreateExamCommand.cs",
  "src/Quizzer.Application/Exams/Commands/CreateDraftVersionCommand.cs",
  "src/Quizzer.Application/Exams/Commands/PublishVersionCommand.cs",
  "src/Quizzer.Application/Exams/Commands/SoftDeleteExamCommand.cs",
  "src/Quizzer.Application/Exams/Commands/RestoreExamCommand.cs",
  "src/Quizzer.Application/Exams/Commands/ImportDraftFromCsvCommand.cs",
  "src/Quizzer.Application/Exams/Queries/GetExamListQuery.cs",
  "src/Quizzer.Application/Exams/Queries/GetExamDetailQuery.cs",
  "src/Quizzer.Application/Attempts/Commands/StartAttemptCommand.cs",
  "src/Quizzer.Application/Attempts/Commands/SaveAnswerCommand.cs",
  "src/Quizzer.Application/Attempts/Commands/FinishAttemptCommand.cs",
  "src/Quizzer.Application/Attempts/Queries/GetAttemptHistoryQuery.cs",
  "src/Quizzer.Application/Attempts/Queries/GetAttemptDetailQuery.cs",
  "src/Quizzer.Application/Reports/Queries/GetExamDashboardQuery.cs",
  "src/Quizzer.Application/Reports/Queries/GetDueQuestionsQuery.cs",
  "src/Quizzer.Application/Reports/Queries/GetWeakQuestionsQuery.cs",
  "src/Quizzer.Application/Study/StudySettings.cs",
  "src/Quizzer.Application/Study/SpacedRepetitionUpdater.cs"
)

foreach ($p in $placeholders) {
  Ensure-File $p "namespace Quizzer.Application; // TODO: implementar`r`n"
}

# 7) Infrastructure stubs
Ensure-File "src/Quizzer.Infrastructure/DependencyInjection.cs" @"
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

        services.AddDbContext<QuizzerDbContext>((sp, opts) =>
        {
            var dbPath = sp.GetRequiredService<DbPathProvider>().GetDbPath();
            opts.UseSqlite(`$""Data Source={dbPath}"" );
        });

        services.AddScoped<IQuizzerDbContext>(sp => sp.GetRequiredService<QuizzerDbContext>());

        return services;
    }
}
"@

Ensure-File "src/Quizzer.Infrastructure/Services/SystemClock.cs" @"
using Quizzer.Application.Abstractions;

namespace Quizzer.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
"@

Ensure-File "src/Quizzer.Infrastructure/Services/DbPathProvider.cs" @"
namespace Quizzer.Infrastructure.Services;

public sealed class DbPathProvider
{
    public string GetDbPath()
    {
        // TODO: definir ubicación definitiva (AppData/Local/Quizzer/quizzer.db)
        var baseDir = AppContext.BaseDirectory;
        return Path.Combine(baseDir, ""quizzer.db"");
    }
}
"@

Ensure-File "src/Quizzer.Infrastructure/Persistence/DesignTimeDbContextFactory.cs" @"
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Quizzer.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<QuizzerDbContext>
{
    public QuizzerDbContext CreateDbContext(string[] args)
    {
        var opts = new DbContextOptionsBuilder<QuizzerDbContext>()
            .UseSqlite(""Data Source=quizzer.db"")
            .Options;

        return new QuizzerDbContext(opts);
    }
}
"@

Ensure-File "src/Quizzer.Infrastructure/Persistence/Migrations/_keep.txt" "placeholder"

# EF Config stubs
$cfgs = @(
  "ExamConfiguration.cs","ExamVersionConfiguration.cs","QuestionConfiguration.cs","OptionConfiguration.cs","AttemptConfiguration.cs","AttemptAnswerConfiguration.cs"
)
foreach ($c in $cfgs) {
  Ensure-File ("src/Quizzer.Infrastructure/Persistence/Configurations/" + $c) "namespace Quizzer.Infrastructure.Persistence.Configurations; // TODO: fluent config`r`n"
}

# 8) Desktop: recursos + navegación + servicios + views/viewmodels
Ensure-File "src/Quizzer.Desktop/Resources/Styles.xaml" @"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <!-- TODO: estilos globales -->
</ResourceDictionary>
"@

Ensure-File "src/Quizzer.Desktop/Resources/Theme.xaml" @"
<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
  <!-- TODO: colores/tema -->
</ResourceDictionary>
"@

Ensure-File "src/Quizzer.Desktop/Navigation/INavigationService.cs" @"
namespace Quizzer.Desktop.Navigation;

public interface INavigationService
{
    void Navigate(object viewModel);
    object? Current { get; }
}
"@

Ensure-File "src/Quizzer.Desktop/Navigation/NavigationService.cs" @"
using CommunityToolkit.Mvvm.ComponentModel;

namespace Quizzer.Desktop.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    private object? current;

    public void Navigate(object viewModel) => Current = viewModel;
}
"@

Ensure-File "src/Quizzer.Desktop/Services/DialogService.cs" @"
namespace Quizzer.Desktop.Services;

public sealed class DialogService
{
    // TODO: confirmaciones, errores, etc.
}
"@

Ensure-File "src/Quizzer.Desktop/Services/FileDialogService.cs" @"
namespace Quizzer.Desktop.Services;

public sealed class FileDialogService
{
    // TODO: abrir/guardar CSV
}
"@

Ensure-File "src/Quizzer.Desktop/ViewModels/MainWindowViewModel.cs" @"
using CommunityToolkit.Mvvm.ComponentModel;
using Quizzer.Desktop.Navigation;

namespace Quizzer.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public INavigationService Nav { get; }

    public MainWindowViewModel(INavigationService nav)
    {
        Nav = nav;
    }
}
"@

Ensure-File "src/Quizzer.Desktop/ViewModels/Exams/ExamsListViewModel.cs" "namespace Quizzer.Desktop.ViewModels.Exams; // TODO`r`n"
Ensure-File "src/Quizzer.Desktop/ViewModels/Exams/ExamDetailViewModel.cs" "namespace Quizzer.Desktop.ViewModels.Exams; // TODO`r`n"
Ensure-File "src/Quizzer.Desktop/ViewModels/Editor/ExamEditorViewModel.cs" "namespace Quizzer.Desktop.ViewModels.Editor; // TODO`r`n"
Ensure-File "src/Quizzer.Desktop/ViewModels/Attempt/AttemptRunnerViewModel.cs" "namespace Quizzer.Desktop.ViewModels.Attempt; // TODO`r`n"
Ensure-File "src/Quizzer.Desktop/ViewModels/Reports/ReportsViewModel.cs" "namespace Quizzer.Desktop.ViewModels.Reports; // TODO`r`n"

function Ensure-UserControl([string]$xamlPath, [string]$className, [string]$ns) {
  $csPath = $xamlPath + ".cs"
  $xamlFile = @"
<UserControl x:Class=""$ns.$className""
             xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
    <Grid Margin=""12"">
        <TextBlock Text=""$className"" FontSize=""20"" />
    </Grid>
</UserControl>
"@
  $csFile = @"
namespace $ns;

public partial class $className : System.Windows.Controls.UserControl
{
    public $className()
    {
        InitializeComponent();
    }
}
"@
  Ensure-File $xamlPath $xamlFile
  Ensure-File $csPath $csFile
}

Ensure-UserControl "src/Quizzer.Desktop/Views/Exams/ExamsListView.xaml"     "ExamsListView"     "Quizzer.Desktop.Views.Exams"
Ensure-UserControl "src/Quizzer.Desktop/Views/Exams/ExamDetailView.xaml"    "ExamDetailView"    "Quizzer.Desktop.Views.Exams"
Ensure-UserControl "src/Quizzer.Desktop/Views/Editor/ExamEditorView.xaml"   "ExamEditorView"    "Quizzer.Desktop.Views.Editor"
Ensure-UserControl "src/Quizzer.Desktop/Views/Attempt/AttemptRunnerView.xaml" "AttemptRunnerView" "Quizzer.Desktop.Views.Attempt"
Ensure-UserControl "src/Quizzer.Desktop/Views/Reports/ReportsView.xaml"     "ReportsView"       "Quizzer.Desktop.Views.Reports"

# 9) Tests stubs + sample CSV
Ensure-File "tests/Quizzer.Tests/TestAssets/SampleExam.csv" @"
question;option1;option2;option3;answer;explanation;tags;difficulty
¿Qué hace un índice clustered?;Ordena físicamente la tabla;Crea un hash;Ninguna de las anteriores;1;Clustered define el orden físico;sql,indexes;2
"@

Ensure-File "tests/Quizzer.Tests/Domain/SpacedRepetitionTests.cs" @"
using Shouldly;
using Quizzer.Domain.Study;

namespace Quizzer.Tests.Domain;

public sealed class SpacedRepetitionTests
{
    [Fact]
    public void WrongAnswer_SetsIntervalToOneDay()
    {
        var s = new QuestionStats { QuestionKey = Guid.NewGuid(), IntervalDays = 10, EaseFactor = 2.5 };
        SpacedRepetition.ApplyResult(s, correct: false, now: DateTimeOffset.UtcNow);
        s.IntervalDays.ShouldBe(1);
    }
}
"@

Ensure-File "tests/Quizzer.Tests/Application/CsvImportTests.cs" @"
namespace Quizzer.Tests.Application;

public sealed class CsvImportTests
{
    [Fact]
    public void Placeholder() => Assert.True(true);
}
"@

Write-Host "OK: estructura objetivo creada (y archivos reubicados si estaban en rutas antiguas)."
