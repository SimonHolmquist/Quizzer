namespace Quizzer.Desktop.Navigation;

public interface INavigationService
{
    object? Current { get; }
    bool CanGoBack { get; }

    void Navigate(object viewModel);
    void GoBack();
}
