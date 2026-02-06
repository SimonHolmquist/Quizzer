namespace Quizzer.Desktop.Navigation;

public interface INavigationService
{
    void Navigate(object viewModel);
    object? Current { get; }
}
