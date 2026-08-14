using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System;
using System.Threading.Tasks;
using Bankmanaging.Services;

namespace Bankmanaging.ViewModels;

public partial class ModifierPViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCommand))]
    private string ancienPin = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCommand))]
    private string nouveauPin = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCommand))]
    private string confirmationPin = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    public Compte Compte { get; }

    public ModifierPViewModel(HeaderViewModel headerViewModel, Compte compte)
    {
        _headerViewModel = headerViewModel;
        Compte = compte;
    }

    // Condition pour activer le bouton Enregistrer (champs complets à 4 chiffres)
    private bool CanModifier() =>
        AncienPin.Length == 4 &&
        NouveauPin.Length == 4 &&
        ConfirmationPin.Length == 4;

    [RelayCommand(CanExecute = nameof(CanModifier))]
    private async Task ModifierAsync()
    {
        if (NouveauPin != ConfirmationPin)
        {
            MessageErreur = "Le nouveau PIN et la confirmation ne correspondent pas.";
            return;
        }

        try
        {
            // Interrogation de la base de données PostgreSQL
            Console.WriteLine(Compte.Numero);
            Console.WriteLine("Compte");

            Result result = await ServiceCompte.ChangePinAsync(Compte.Numero, AncienPin, NouveauPin);

            if (result.Status)
            {
                MessageErreur = string.Empty;

                var box = MessageBoxManager.GetMessageBoxStandard(
                    "Succès",
                    result.Message,
                    ButtonEnum.Ok,
                    Icon.Success
                );
                await box.ShowAsync();

                _headerViewModel.Retrait();
            }
            else
            {
                MessageErreur = result.Message;
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Erreur : {ex.Message}";
        }
    }
    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.Retrait();
    }
}