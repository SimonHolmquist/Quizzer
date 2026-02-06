namespace Quizzer.Desktop.Navigation;

public interface INavigationAware
{
    Task OnNavigatedToAsync(object? parameter);
}
