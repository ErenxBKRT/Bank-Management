using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class HeaderViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private ViewModelBase _controlView;

    public HeaderViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
        ControlView = new DashViewModel(this);
    }
    public void ChangerAction(string act)
    {
        ControlView= new ActionViewModel(act, this);
    }
    public void AnnulerAction()
    {
        ControlView = new DashViewModel(this);
    }

    private void Deconnexion()
    {
        _mainViewModel.FermerApplication();
    }
}