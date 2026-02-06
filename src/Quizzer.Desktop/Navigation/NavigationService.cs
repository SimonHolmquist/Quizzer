using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Quizzer.Desktop.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    private readonly Stack<object> _stack = new();

    [ObservableProperty]
    private object? current;

    public bool CanGoBack => _stack.Count > 1;

    public void Navigate(object viewModel)
    {
        _stack.Push(viewModel);
        Current = viewModel;
        OnPropertyChanged(nameof(CanGoBack));
    }

    public void GoBack()
    {
        if (_stack.Count <= 1) return;

        _stack.Pop();
        Current = _stack.Peek();
        OnPropertyChanged(nameof(CanGoBack));
    }
}
