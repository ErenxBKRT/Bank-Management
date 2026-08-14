using Avalonia;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Reflection.Metadata;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class VirementViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence? Agence { get; }

    [ObservableProperty]
    private string numeroCompte = "";

    [ObservableProperty]
    private decimal montant = 0;

    [ObservableProperty]
    private string nomEmetteur = "";

    [ObservableProperty]
    private string description = "";

    [ObservableProperty]
    private string messageErreur = "";

    [ObservableProperty]
    private string messageSucces = "";

    public VirementViewModel(HeaderViewModel headerViewModel, Agence? agence)
    {
        _headerViewModel = headerViewModel;
        Agence = agence;
    }

    [RelayCommand]
    private async Task ValiderVirementAsync()
    {
        MessageErreur = string.Empty;
        MessageSucces = string.Empty;

        if (Agence == null || string.IsNullOrWhiteSpace(Agence.CodeAgence))
        {
            MessageErreur = "Agence non définie ou invalide.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NumeroCompte))
        {
            MessageErreur = "Le numéro de compte destinataire est requis.";
            return;
        }

        if (Montant <= 0)
        {
            MessageErreur = "Le montant doit être supérieur à zéro.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NomEmetteur))
        {
            MessageErreur = "Le nom de l'émetteur est requis.";
            return;
        }

        // Exécution du virement en base de données
        Result res = await CreditVirement.VirementAsync(
            numero: NumeroCompte,
            codeAgence: Agence.CodeAgence,
            montant: Montant,
            nom: NomEmetteur,
            description: Description
        );

        if (res.Status)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(
                    title: "Succès",
                    text: res.Message,
                    ButtonEnum.Ok,
                    Icon.Success
                );

            //Wait for the user to close the message box before navigating back to the client menu
            await box.ShowAsync();
            _headerViewModel.MainMenu();
            ReinitialiserChamps();
        }
        else
        {
            MessageErreur = res.Message;
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.Transaction();
    }

    private void ReinitialiserChamps()
    {
        NumeroCompte = string.Empty;
        Montant = 0;
        NomEmetteur = string.Empty;
        Description = string.Empty;
    }
}