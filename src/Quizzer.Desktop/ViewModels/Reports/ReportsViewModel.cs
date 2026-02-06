using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Quizzer.Application.Reports.Queries;
using Quizzer.Desktop.Navigation;

namespace Quizzer.Desktop.ViewModels.Reports;

public sealed partial class ReportsViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    private readonly INavigationService _nav;

    private Guid? _examId;

    public ReportsViewModel(IMediator mediator, INavigationService nav)
    {
        _mediator = mediator;
        _nav = nav;
    }

    [ObservableProperty] private string header = "Reportes";
    [ObservableProperty] private string subheader = "";
    [ObservableProperty] private ExamDashboardItemVm? selectedExam;

    public IList<ExamDashboardItemVm> Exams { get; private set; } = [];
    public IList<WeakQuestionVm> WeakQuestions { get; private set; } = [];
    public IList<DueQuestionVm> DueQuestions { get; private set; } = [];

    public async Task LoadAsync(Guid? examId = null)
    {
        _examId = examId;
        await RefreshAsync();
    }

    [RelayCommand]
    private void Back() => _nav.GoBack();

    [RelayCommand]
    private async Task Refresh() => await RefreshAsync();

    partial void OnSelectedExamChanged(ExamDashboardItemVm? value)
    {
        if (value is null) return;
        _examId = value.ExamId;
        _ = LoadExamDetailsAsync(value.ExamId);
    }

    private async Task RefreshAsync()
    {
        var items = await _mediator.Send(new GetExamDashboardQuery());
        Exams = items.Select(i => new ExamDashboardItemVm(i.ExamId, i.Name, i.TotalAttempts, i.AverageScorePercent, i.LastAttemptAt)).ToList();
        OnPropertyChanged(nameof(Exams));

        SelectedExam = _examId is null
            ? Exams.FirstOrDefault()
            : Exams.FirstOrDefault(x => x.ExamId == _examId) ?? Exams.FirstOrDefault();

        if (SelectedExam is not null)
            await LoadExamDetailsAsync(SelectedExam.ExamId);
    }

    private async Task LoadExamDetailsAsync(Guid examId)
    {
        var weak = await _mediator.Send(new GetWeakQuestionsQuery(examId));
        WeakQuestions = weak.Select(w => new WeakQuestionVm(w.QuestionId, w.QuestionText, w.TotalAnswers, w.CorrectAnswers, w.AccuracyPercent)).ToList();
        OnPropertyChanged(nameof(WeakQuestions));

        var due = await _mediator.Send(new GetDueQuestionsQuery(examId));
        DueQuestions = due.Select(d => new DueQuestionVm(d.QuestionId, d.QuestionText, d.LastAnsweredAt)).ToList();
        OnPropertyChanged(nameof(DueQuestions));

        Subheader = SelectedExam is null
            ? ""
            : $"{SelectedExam.Name} · {SelectedExam.TotalAttempts} intentos";
    }
}

public sealed record ExamDashboardItemVm(Guid ExamId, string Name, int TotalAttempts, double AverageScore, DateTimeOffset? LastAttemptAt)
{
    public string AverageScoreLabel => $"{AverageScore:0.0}%";
    public string LastAttemptLabel => LastAttemptAt is null ? "-" : LastAttemptAt.Value.ToLocalTime().ToString("g");
}

public sealed record WeakQuestionVm(Guid QuestionId, string Text, int TotalAnswers, int CorrectAnswers, double AccuracyPercent)
{
    public string AccuracyLabel => $"{AccuracyPercent:0.0}%";
}

public sealed record DueQuestionVm(Guid QuestionId, string Text, DateTimeOffset? LastAnsweredAt)
{
    public string LastAnsweredLabel => LastAnsweredAt is null ? "Nunca" : LastAnsweredAt.Value.ToLocalTime().ToString("g");
}
