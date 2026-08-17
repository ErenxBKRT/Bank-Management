using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using Bankmanaging.Services;


namespace Bankmanaging.ViewModels;

public partial class RapportViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence Agence {get;}

    public Releve Rel {get;set;}
     
    [ObservableProperty]
    private decimal solde= 0;

    [ObservableProperty]
    private int virement;

    [ObservableProperty]
    private int credit;

    [ObservableProperty]
    private int remboursement;

    [ObservableProperty]
    private int depot;

    public RapportViewModel (HeaderViewModel headerViewModel,Agence agence)
    {
        _headerViewModel= headerViewModel;
        Agence = agence;
        Solde = Agence.Solde;
        Rel = new Releve(Agence.CodeAgence);
        _= rp();
    }

     public async Task rp()
    {
        await Rel.GetTotalTransactionAsync();

        Virement = Rel.Virement;
        Credit = Rel.Credit;
        Remboursement = Rel.Remboursement;
        Depot = Rel.Depot;
    }

    [RelayCommand]
    public void Retour()
    {
        _headerViewModel.MainMenu();
    }
}
