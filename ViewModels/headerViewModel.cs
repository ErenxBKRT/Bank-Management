using Bankmanaging.Models;
using Bankmanaging.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;


namespace Bankmanaging.ViewModels;

public partial class HeaderViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private ViewModelBase? _controlView;

    [ObservableProperty]
    private string _ce;

    public Compte? Compte1 {get;}
    public Agence? Agence1 {get;}

    public HeaderViewModel (MainViewModel mainViewModel, string mode, Compte? compte = null, Agence? agence = null)
    {
        _mainViewModel = mainViewModel;
        Ce = mode;

        //get info account from session if not provided
        Compte1 =  SessionServ.CurrentCompte;
        Agence1 =  SessionServ.CurrentAgence;


        if (mode == "E")
        {
            ControlView = new DashViewModel(this);
        }
        if (mode == "C")
        {
            ControlView = new DCViewModel(this, Compte1);
        }
    }
    public void MenuClient()
    {
        ControlView = new ClientsViewModel(this);
    }
    public void EntrerClient(Compte compte)
    {
        ControlView = new DCViewModel(this, Compte1);
    }

    public void OuvrirClient(Client client)
    {
        ControlView = new ClientDetailViewModel(this, client);
    }
  
  public void ModifierClient(Client client)
    {
        ControlView = new ModifierClientViewModel(this, client);
    }

    public void MainMenu()
    {
        ControlView = new DashViewModel(this);
    }

    public void Retrait()
    {
        ControlView = new DCViewModel(this, Compte1);
    }

    public void ModifierP()
    {
        Console.WriteLine(Compte1.Credit);
        ControlView = new ModifierPViewModel(this, Compte1);
    }

    public void NouveauC()
    {
        ControlView = new CreationCViewModel(this);
    }

    public void CCompte()
    {
        ControlView = new CCompteViewModel(this);
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
        ControlView = new VirementViewModel(this, Agence1);
    }

    public void Depot()
    {
        ControlView = new DepotViewModel(this, Agence1);
    }

    public void Credit()
    {
        ControlView = new CreditViewModel(this, Agence1);
    }

    public void Rembourser()
    {
        ControlView = new RembourserViewModel(this, Agence1);
    }
    
    [RelayCommand]
    private void Deconnexion()
    {
        _mainViewModel.Deco();
    }
}