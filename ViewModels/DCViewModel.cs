using System.Threading.Tasks;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Compte Compte { get; }

    [ObservableProperty]
    private decimal _somme = 0;

    [ObservableProperty]
    private decimal _solde = 0;

    [ObservableProperty]
    private decimal _credit = 0;

    [ObservableProperty]
    private string _PIN = string.Empty;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy; // IsBusy est utilisé pour désactiver les boutons pendant l'exécution d'une commande asynchrone.

    //HeaderViewModel : new DCViewModel(this, Compte1)
    public DCViewModel(HeaderViewModel headerViewModel, Compte compte)
    {
        _headerViewModel = headerViewModel;
        Compte = compte;

        Solde = compte.Solde;
        Credit = compte.Credit;
    }

    [RelayCommand]
    private async Task RetraitAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            Result result = await DepotRetrait.WithdrawAsync(Compte.Numero, PIN, Somme);
            Message = result.Message;

            if (result.Status)
            {
                Solde -= Somme;
                Compte.Solde = Solde;
                Somme = 0;
                PIN = string.Empty;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void ModifierP()
    {
        _headerViewModel.ModifierP();
    }
}
