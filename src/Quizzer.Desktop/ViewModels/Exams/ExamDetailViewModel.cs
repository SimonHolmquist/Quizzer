using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quizzer.Application.Exams.Commands;
using Quizzer.Application.Exams.Queries;
using Quizzer.Desktop.Navigation;
using Quizzer.Desktop.ViewModels.Attempt;
using Quizzer.Desktop.ViewModels.Editor;
using Quizzer.Desktop.ViewModels.Reports;

namespace Quizzer.Desktop.ViewModels.Exams;

public sealed partial class ExamDetailViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly INavigationService _nav;
    private readonly IServiceProvider _sp;

    private Guid _examId;
    private Guid? _publishedVersionId;

    public ExamDetailViewModel(IMediator mediator, INavigationService nav, IServiceProvider sp)
    {
        _mediator = mediator;
        _nav = nav;
        _sp = sp;
    }

    [ObservableProperty] private string title = "";
    [ObservableProperty] private string subtitle = "";
    [ObservableProperty] private string draftLabel = "";
    [ObservableProperty] private string publishedLabel = "";
    [ObservableProperty] private string questionCountLabel = "";
    [ObservableProperty] private bool hasDraft;
    [ObservableProperty] private bool hasPublished;

    public async Task LoadAsync(Guid examId)
    {
        _examId = examId;
        await RefreshAsync();
    }

    [RelayCommand]
    private void Back() => _nav.GoBack();

    [RelayCommand]
    private async Task Refresh() => await RefreshAsync();

    [RelayCommand]
    private async Task OpenDraftEditor()
    {
        await _mediator.Send(new CreateDraftVersionCommand(_examId));

        var editor = _sp.GetRequiredService<ExamEditorViewModel>();
        await editor.LoadAsync(_examId);
        _nav.Navigate(editor);
    }

    [RelayCommand]
    private async Task StartAttempt()
    {
        if (_publishedVersionId is null && !HasDraft) return;

        var attemptVm = _sp.GetRequiredService<AttemptRunnerViewModel>();
        await attemptVm.LoadAsync(_examId);
        _nav.Navigate(attemptVm);
    }

    [RelayCommand]
    private async Task OpenReports()
    {
        var reportsVm = _sp.GetRequiredService<ReportsViewModel>();
        await reportsVm.LoadAsync(_examId);
        _nav.Navigate(reportsVm);
    }

    private async Task RefreshAsync()
    {
        var detail = await _mediator.Send(new GetExamDetailQuery(_examId));
        var latestPublished = await _mediator.Send(new GetLatestPublishedVersionQuery(_examId));

        Title = detail.Name;
        Subtitle = "Detalle de examen";

        HasDraft = detail.DraftVersion is not null;
        DraftLabel = detail.DraftVersion is null
            ? "Draft: No disponible"
            : $"Draft: v{detail.DraftVersion.VersionNumber}";

        QuestionCountLabel = detail.DraftVersion is null
            ? "Preguntas en Draft: -"
            : $"Preguntas en Draft: {detail.DraftVersion.Questions.Count}";

        _publishedVersionId = latestPublished?.VersionId;
        HasPublished = latestPublished is not null;
        PublishedLabel = latestPublished is null
            ? "Publicado: -"
            : $"Publicado: v{latestPublished.VersionNumber}";

        OnPropertyChanged(nameof(CanStartAttempt));
    }

    public bool CanStartAttempt => HasPublished || HasDraft;
}
