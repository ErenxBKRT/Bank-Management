using Avalonia.Controls;
using Bankmanaging.Models;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class HeaderViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private ViewModelBase _controlView;

    [ObservableProperty]
    private string _ce;

    public HeaderViewModel(MainViewModel mainViewModel,string mode)
    {
        _mainViewModel = mainViewModel;
        Ce = mode;
        if (mode == "E")
        {
            ControlView = new DashViewModel(this);
        }
        if (mode == "C")
        {
            ControlView = new DCViewModel(this);
        }
    }
    public void MenuClient()
    {
        ControlView= new ClientsViewModel(this);
    }
    public void EntrerClient()
    {
        ControlView = new DCViewModel(this);
    }

    public void OuvrirClient(Client client)
    {
        ControlView = new ClientDetailViewModel(this, client);
    }
    public void ActionClient(string action)
    {
        ControlView = new ActionViewModel(this,action);
    }
    public void MainMenu()
    {
        ControlView = new DashViewModel(this);
    }

    public void NouveauC()
    {
        ControlView = new CreationCViewModel(this);
    }
    public void MenuHistorique()
    {
        ControlView = new HistoriqueViewModel(this);
    }
    public void MenuAgence()
    {
        ControlView = new AgenceViewModel(this);
    }
    public void NouveauA()
    {
        ControlView = new CreationAViewModel(this);
    }

    public void Transaction()
    {
        ControlView = new TransactionViewModel(this);
    }

    public void Virement()
    {
        ControlView = new VirementViewModel(this);
    }

    public void Depot()
    {
        ControlView = new DepotViewModel(this);
    }

    public void Credit()
    {
        ControlView = new CreditViewModel(this);
    }
    
    [RelayCommand]
    private void Deconnexion()
    {
        _mainViewModel.Deco();
    }
}