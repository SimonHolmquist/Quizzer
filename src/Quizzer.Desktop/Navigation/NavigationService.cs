using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Quizzer.Desktop.Navigation;

public sealed partial class NavigationService : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Stack<object> _stack = new();

    [ObservableProperty]
    private object? current;

    public NavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public bool CanGoBack => _stack.Count > 1;

    public void Navigate(object viewModel)
    {
        _stack.Push(viewModel);
        Current = viewModel;
        OnPropertyChanged(nameof(CanGoBack));
    }

    public async Task NavigateToAsync<TViewModel>(object? parameter = null) where TViewModel : class
    {
        var viewModel = _serviceProvider.GetRequiredService<TViewModel>();
        if (viewModel is INavigationAware aware)
            await aware.OnNavigatedToAsync(parameter);

        Navigate(viewModel);
    }

    public void GoBack()
    {
        if (_stack.Count <= 1) return;

        _stack.Pop();
        Current = _stack.Peek();
        OnPropertyChanged(nameof(CanGoBack));
    }
}
