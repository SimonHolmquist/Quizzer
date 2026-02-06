using CommunityToolkit.Mvvm.ComponentModel;

namespace Quizzer.Desktop.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    [ObservableProperty]
    private object? current;

    public void Navigate(object viewModel) => Current = viewModel;
}
