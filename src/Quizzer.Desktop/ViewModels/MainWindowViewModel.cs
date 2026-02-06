using CommunityToolkit.Mvvm.ComponentModel;
using Quizzer.Desktop.Navigation;

namespace Quizzer.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public INavigationService Nav { get; }

    public MainWindowViewModel(INavigationService nav)
    {
        Nav = nav;
    }
}
