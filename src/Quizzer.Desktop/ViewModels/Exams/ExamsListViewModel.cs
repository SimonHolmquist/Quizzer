using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Quizzer.Application.Exams.Commands;
using Quizzer.Application.Exams.Queries;
using Quizzer.Desktop.Navigation;
using Quizzer.Desktop.ViewModels.Editor;

namespace Quizzer.Desktop.ViewModels.Exams;

public sealed partial class ExamsListViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly INavigationService _nav;
    private readonly IServiceProvider _sp;

    public ExamsListViewModel(IMediator mediator, INavigationService nav, IServiceProvider sp)
    {
        _mediator = mediator;
        _nav = nav;
        _sp = sp;

        RefreshCommand.Execute(null);
    }

    [ObservableProperty] private string newExamName = "";
    [ObservableProperty] private ExamListItemVm? selectedExam;

    public IList<ExamListItemVm> Exams { get; private set; } = new List<ExamListItemVm>();

    [RelayCommand]
    private async Task Refresh()
    {
        var items = await _mediator.Send(new GetExamListQuery());
        Exams = items.Select(x => new ExamListItemVm(x.ExamId, x.Name, x.LatestPublishedVersionNumber, x.HasDraft)).ToList();
        OnPropertyChanged(nameof(Exams));
    }

    [RelayCommand]
    private async Task CreateExam()
    {
        var name = NewExamName?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return;

        var examId = await _mediator.Send(new CreateExamCommand(name));
        await _mediator.Send(new CreateDraftVersionCommand(examId));

        NewExamName = "";
        await Refresh();

        await OpenEditorInternal(examId);
    }

    [RelayCommand]
    private async Task OpenEditor()
    {
        if (SelectedExam is null) return;
        await OpenEditorInternal(SelectedExam.ExamId);
    }

    private async Task OpenEditorInternal(Guid examId)
    {
        await _mediator.Send(new CreateDraftVersionCommand(examId)); // idempotente

        var editor = _sp.GetRequiredService<ExamEditorViewModel>(); // transient
        await editor.LoadAsync(examId);

        _nav.Navigate(editor);
    }
}

public sealed record ExamListItemVm(Guid ExamId, string Name, int? LatestPublishedVersionNumber, bool HasDraft)
{
    public string LatestVersionLabel => LatestPublishedVersionNumber is null ? "-" : $"v{LatestPublishedVersionNumber}";
    public string HasDraftLabel => HasDraft ? "Sí" : "No";
}
