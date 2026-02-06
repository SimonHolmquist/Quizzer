using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Quizzer.Application.Attempts.Commands;
using Quizzer.Application.Attempts.Queries;
using Quizzer.Desktop.Navigation;

namespace Quizzer.Desktop.ViewModels.Attempt;

public sealed partial class AttemptRunnerViewModel(IMediator mediator, INavigationService nav) : ObservableObject
{
    private readonly IMediator _mediator = mediator;
    private readonly INavigationService _nav = nav;

    private Guid _examId;
    private Guid _attemptId;
    private DateTimeOffset _questionStartedAt = DateTimeOffset.UtcNow;
    [ObservableProperty] private string header = "";
    [ObservableProperty] private string subheader = "";
    [ObservableProperty] private string progressLabel = "";
    [ObservableProperty] private string resultSummary = "";
    [ObservableProperty] private AttemptQuestionVm? currentQuestion;
    [ObservableProperty] private bool canGoPrevious;
    [ObservableProperty] private bool canGoNext;
    [ObservableProperty] private bool canFinish;
    [ObservableProperty] private bool isFinished;

    public IList<AttemptQuestionVm> Questions { get; private set; } = [];

    public async Task LoadAsync(Guid examId)
    {
        _examId = examId;
        await StartAsync();
    }

    [RelayCommand]
    private void Back() => _nav.GoBack();

    [RelayCommand]
    private async Task Start() => await StartAsync();

    [RelayCommand]
    private async Task Previous()
    {
        if (!CanGoPrevious || CurrentQuestion is null) return;

        await SaveCurrentAnswerAsync();
        SetCurrentIndex(CurrentIndex - 1);
    }

    [RelayCommand]
    private async Task Next()
    {
        if (!CanGoNext || CurrentQuestion is null) return;

        await SaveCurrentAnswerAsync();
        SetCurrentIndex(CurrentIndex + 1);
    }

    [RelayCommand]
    private async Task Finish()
    {
        if (CurrentQuestion is null || _attemptId == Guid.Empty) return;

        await SaveCurrentAnswerAsync();

        var result = await _mediator.Send(new FinishAttemptCommand(_attemptId));
        ResultSummary = $"Resultado: {result.CorrectCount}/{result.TotalCount} ({result.ScorePercent:0.0}%)";
        IsFinished = true;
    }

    private int CurrentIndex { get; set; }

    private async Task StartAsync()
    {
        IsFinished = false;
        ResultSummary = "";

        var session = await _mediator.Send(new StartAttemptCommand(_examId));
        _attemptId = session.AttemptId;

        var detail = await _mediator.Send(new GetAttemptDetailQuery(_attemptId));

        Header = detail.ExamName;
        Subheader = $"v{detail.VersionNumber}";

        Questions = detail.Questions
            .Select(q => new AttemptQuestionVm(q))
            .ToList();

        OnPropertyChanged(nameof(Questions));

        CurrentIndex = 0;
        SetCurrentIndex(0);
    }

    private void SetCurrentIndex(int index)
    {
        if (Questions.Count == 0)
        {
            CurrentQuestion = null;
            ProgressLabel = "Sin preguntas";
            CanGoPrevious = false;
            CanGoNext = false;
            CanFinish = false;
            return;
        }

        CurrentIndex = Math.Clamp(index, 0, Questions.Count - 1);
        CurrentQuestion = Questions[CurrentIndex];

        CanGoPrevious = CurrentIndex > 0;
        CanGoNext = CurrentIndex < Questions.Count - 1;
        CanFinish = Questions.Count > 0;

        ProgressLabel = $"{CurrentIndex + 1} / {Questions.Count}";
        _questionStartedAt = DateTimeOffset.UtcNow;
    }

    private async Task SaveCurrentAnswerAsync()
    {
        if (CurrentQuestion?.SelectedOption is null || _attemptId == Guid.Empty) return;

        var seconds = (int)Math.Max(0, (DateTimeOffset.UtcNow - _questionStartedAt).TotalSeconds);

        await _mediator.Send(new SaveAnswerCommand(
            _attemptId,
            CurrentQuestion.QuestionId,
            CurrentQuestion.SelectedOption.OptionId,
            seconds,
            CurrentQuestion.FlaggedDoubt));
    }
}

public sealed partial class AttemptQuestionVm : ObservableObject
{
    public AttemptQuestionVm(AttemptQuestionDetailDto question)
    {
        QuestionId = question.QuestionId;
        Text = question.Text;
        foreach (var opt in question.Options)
            Options.Add(new AttemptOptionVm(opt.OptionId, opt.Text));
    }

    public Guid QuestionId { get; }

    [ObservableProperty] private string text = "";

    public IList<AttemptOptionVm> Options { get; } = [];

    [ObservableProperty] private AttemptOptionVm? selectedOption;

    [ObservableProperty] private bool flaggedDoubt;
}

public sealed record AttemptOptionVm(Guid OptionId, string Text);
