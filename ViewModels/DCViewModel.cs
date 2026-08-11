using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Compte? Compte {get;}

    [ObservableProperty]
    private decimal somme=0;

    [ObservableProperty]
    private decimal solde=0;

    [ObservableProperty]
    private decimal credit=0;

    public DCViewModel (HeaderViewModel headerViewModel,Compte? compte)
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