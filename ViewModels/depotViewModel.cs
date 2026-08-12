using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class DepotViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence Agence { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmerCommand))] // <-- Modifié ici
    private string numeroCompte = "";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmerCommand))] // <-- Modifié ici
    private decimal somme = 0;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmerCommand))] // <-- Modifié ici
    private string pin = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    public DepotViewModel(HeaderViewModel headerViewModel, Agence agence)
    {
        _headerViewModel = headerViewModel;
        Agence = agence;
    }

    private bool CanValider() =>
        !string.IsNullOrWhiteSpace(NumeroCompte) &&
        !string.IsNullOrWhiteSpace(Pin) &&
        Somme > 0;

    [RelayCommand(CanExecute = nameof(CanValider))]
    private async Task ConfirmerAsync()
    {
        try
        {
            Result result = await DepotRetrait.DepositAsync(NumeroCompte, Somme, Pin);
            if (!result.Status)
            {
                MessageErreur = result.Message;
            }
            else
            {
                //Popup de confirmation
                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: "Succès",
                    text: result.Message,
                    ButtonEnum.Ok,
                    Icon.Success
                );

                //Wait for the user to close the message box before navigating back to the client menu
                await box.ShowAsync();
                _headerViewModel.Transaction();
            }


        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.Transaction();
    }
}