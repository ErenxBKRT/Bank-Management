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
    public void MenuClient()
    {
        ControlView= new ClientViewModel(this);
    }
    public void EntrerClient()
    {
        ControlView = new DRSCViewModel(this);
    }
    public void ActionClient(string action)
    {
        ControlView = new ActionViewModel(this,action);
    }
    public void MainMenu()
    {
        ControlView = new DashViewModel(this);
    }

    public void Nouveau(string qui)
    {
        ControlView = new NouveauViewModel(this, qui);
    }
    public void MenuEmploye()
    {
        ControlView = new EmployeViewModel(this);
    }

    public void MenuAgence()
    {
        ControlView = new AgenceViewModel(this);
    }
    public void MenuHistorique()
    {
        ControlView = new HistoriqueViewModel(this);
    }
    
    [RelayCommand]
    private void Deconnexion()
    {
        _mainViewModel.Deco();
    }
}