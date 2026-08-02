using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase currentView;

    public MainViewModel()
    {
        CurrentView = new ConnexionViewModel(this);
    }

    public void OuvrirApplication()
    {
        CurrentView = new HeaderViewModel(this);
    }

    public void Deco()
    {
        CurrentView = new ConnexionViewModel(this);
    }
}