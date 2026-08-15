using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;

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
    private string _message = string.Empty;

    [ObservableProperty]
    private string _errorMsg = string.Empty;

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
            Result result = await DepotRetrait.WithdrawAsync(Compte.Numero, Somme);
            Message = result.Message;

            if (result.Status)
            {
                Solde -= Somme;
                Compte.Solde = Solde;
                Somme = 0;
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Erreur",
                    result.Message,
                    ButtonEnum.Ok,
                    Icon.Success
                );
                await box.ShowAsync();
            }
            else
            {
                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Succes",
                    result.Message,
                    ButtonEnum.Ok,
                    Icon.Error
                );
                await box.ShowAsync();
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
