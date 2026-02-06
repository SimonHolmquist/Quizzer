namespace Quizzer.Desktop.Navigation;

public interface INavigationService
{
    object? Current { get; }
    bool CanGoBack { get; }

    void Navigate(object viewModel);
    Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : class;
    void GoBack();
}
