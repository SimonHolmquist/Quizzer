using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using Quizzer.Application.Exams.Commands;
using Quizzer.Application.Exams.Queries;
using Quizzer.Desktop.Navigation;
using Quizzer.Desktop.ViewModels.Exams;

namespace Quizzer.Desktop.ViewModels.Editor;

public sealed partial class ExamEditorViewModel(IMediator mediator, INavigationService nav, ExamsListViewModel examsVm) : ObservableObject
{
    private readonly IMediator _mediator = mediator;
    private readonly INavigationService _nav = nav;

    private Guid _examId;
    private Guid _draftVersionId;

    private readonly ExamsListViewModel _examsVm = examsVm;

    [RelayCommand]
    private void Back() => _nav.GoBack();

    [ObservableProperty] private string header = "";
    [ObservableProperty] private string subheader = "";
    [ObservableProperty] private string publishNotes = "";

    public IList<QuestionEditVm> Questions { get; private set; } = [];

    [ObservableProperty] private QuestionEditVm? selectedQuestion;

    public async Task LoadAsync(Guid examId)
    {
        _examId = examId;

        var dto = await _mediator.Send(new GetExamDetailQuery(examId));
        if (dto.DraftVersion is null)
            throw new InvalidOperationException("No hay Draft. (CreateDraftVersionCommand debería haberlo creado).");

        _draftVersionId = dto.DraftVersion.VersionId;

        Header = dto.Name;
        Subheader = $"Draft v{dto.DraftVersion.VersionNumber} (aún no publicado)";

        Questions = [.. dto.DraftVersion.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => QuestionEditVm.FromDto(q))];

        OnPropertyChanged(nameof(Questions));
        SelectedQuestion = Questions.FirstOrDefault();
    }

    [RelayCommand]
    private void AddQuestion()
    {
        var nextOrder = Questions.Count == 0 ? 1 : Questions.Max(q => q.OrderIndex) + 1;

        var q = new QuestionEditVm
        {
            QuestionKey = Guid.NewGuid(),
            OrderIndex = nextOrder,
            Text = "Nueva pregunta..."
        };

        q.Options.Add(new OptionEditVm { OptionKey = Guid.NewGuid(), OrderIndex = 1, Text = "Opción 1", IsCorrect = true });
        q.Options.Add(new OptionEditVm { OptionKey = Guid.NewGuid(), OrderIndex = 2, Text = "Opción 2", IsCorrect = false });

        Questions = [.. Questions, q];
        OnPropertyChanged(nameof(Questions));
        SelectedQuestion = q;
    }

    [RelayCommand]
    private void AddOption()
    {
        if (SelectedQuestion is null) return;

        var next = SelectedQuestion.Options.Count == 0 ? 1 : SelectedQuestion.Options.Max(o => o.OrderIndex) + 1;
        SelectedQuestion.Options.Add(new OptionEditVm { OptionKey = Guid.NewGuid(), OrderIndex = next, Text = $"Opción {next}" });

        if (!SelectedQuestion.Options.Any(o => o.IsCorrect))
            SelectedQuestion.Options[0].IsCorrect = true;
    }

    [RelayCommand]
    private async Task SaveDraft()
    {
        var cmd = new UpsertDraftContentCommand(
            _draftVersionId,
            [.. Questions.Select(q => q.ToDto())]
        );

        await _mediator.Send(cmd);

        // recargar para reflejar normalizaciones
        await LoadAsync(_examId);
    }

    [RelayCommand]
    private async Task Publish()
    {
        var notes = PublishNotes?.Trim();
        if (string.IsNullOrWhiteSpace(notes)) return;

        await SaveDraft();

        await _mediator.Send(new PublishVersionCommand(_draftVersionId, notes));
        PublishNotes = "";

        // volver a lista
        Back();
    }
}

public sealed partial class QuestionEditVm : ObservableObject
{
    public Guid QuestionKey { get; set; }
    public int OrderIndex { get; set; }

    [ObservableProperty] private string text = "";

    public IList<OptionEditVm> Options { get; } = [];

    public string Title => $"{OrderIndex}. {Truncate(Text, 48)}";

    public ExamQuestionDto ToDto()
    {
        // fuerza 1 correcta: si hay varias, se queda con la primera marcada
        var firstCorrect = Options.FirstOrDefault(o => o.IsCorrect) ?? Options.FirstOrDefault();
        foreach (var o in Options)
            o.IsCorrect = ReferenceEquals(o, firstCorrect);

        return new ExamQuestionDto(
            QuestionKey,
            OrderIndex,
            Text,
            [.. Options.Select(o => o.ToDto())]
        );
    }

    public static QuestionEditVm FromDto(ExamQuestionDto dto)
    {
        var vm = new QuestionEditVm
        {
            QuestionKey = dto.QuestionKey,
            OrderIndex = dto.OrderIndex,
            Text = dto.Text
        };

        foreach (var o in dto.Options.OrderBy(x => x.OrderIndex))
            vm.Options.Add(new OptionEditVm
            {
                OptionKey = o.OptionKey,
                OrderIndex = o.OrderIndex,
                Text = o.Text,
                IsCorrect = o.IsCorrect
            });

        return vm;
    }

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
}

public sealed partial class OptionEditVm : ObservableObject
{
    public Guid OptionKey { get; set; }
    public int OrderIndex { get; set; }

    [ObservableProperty] private string text = "";

    private bool _isCorrect;

    public bool IsCorrect
    {
        get => _isCorrect;
        set
        {
            if (SetProperty(ref _isCorrect, value) && value)
            {
                // cuando marcás una como correcta, desmarca el resto
                // (lo hace el binding, pero esto asegura invariantes en memoria)
                var parent = FindParentQuestion();
                if (parent is not null)
                {
                    foreach (var o in parent.Options)
                        if (!ReferenceEquals(o, this))
                            o._isCorrect = false;
                }
            }
        }
    }

    public ExamOptionDto ToDto() => new(OptionKey, OrderIndex, Text, IsCorrect);

    private static QuestionEditVm? FindParentQuestion() => null; // simplificado: invariantes se corrigen al salvar
}
