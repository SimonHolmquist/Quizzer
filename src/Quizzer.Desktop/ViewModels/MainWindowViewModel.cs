using CommunityToolkit.Mvvm.ComponentModel;
using Quizzer.Desktop.Navigation;

namespace Quizzer.Desktop.ViewModels;

public sealed partial class MainWindowViewModel : ObservableObject
{
    public NavigationService Nav { get; }

    [ObservableProperty]
    private string statusText = "";

    public MainWindowViewModel(NavigationService nav)
    {
        Nav = nav;
        StatusText = "DB: LocalAppData\\Quizzer\\quizzer.db";
    }
}
