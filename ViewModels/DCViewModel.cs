using System.Net.Http.Headers;
using Bankmanaging.Models;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private decimal somme=0;

    [ObservableProperty]
    private decimal solde=0;

    [ObservableProperty]
    private decimal credit=0;

    public Compte Compte {get;}

    public DCViewModel (HeaderViewModel headerViewModel,Compte compte)
    {
        _headerViewModel= headerViewModel;
        Compte = compte;
    }

    [RelayCommand]
    private void Retrait()
    {
        //code qui fait le retrait
    }

    [RelayCommand]
    private void ModifierP()
    {
        _headerViewModel.ModifierP();
    }

}