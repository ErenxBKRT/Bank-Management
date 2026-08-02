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
    
    public void ConnecterClient()
    {
        ControlView= new ClientLogViewModel(this);
    }
    public void RevenirClient()
    {
        ControlView= new ClientViewModel(this);
    }
    public void ActionClient()
    {
        ControlView = new DRSCViewModel(this);
    }
    public void MainMenu()
    {
        ControlView = new DashViewModel(this);
    }
    
    [RelayCommand]
    private void Deconnexion()
    {
        _mainViewModel.Deco();
    }
}