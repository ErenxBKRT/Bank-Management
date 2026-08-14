using System.Threading.Tasks;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace Bankmanaging.ViewModels;

public partial class RembourserViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence? Agence { get; }

    [ObservableProperty]
    private decimal montant = 0;

    [ObservableProperty]
    private string numeroCompte = "";

    public RembourserViewModel(HeaderViewModel headerViewModel, Agence? agence)
    {
        _headerViewModel = headerViewModel;
        Agence = agence;
    }

    [RelayCommand]
    private async Task ValiderRemboursementAsync()
    {
        if (Agence == null || string.IsNullOrWhiteSpace(Agence.CodeAgence))
        {
            await AfficherPopupAsync("Erreur", "Agence non définie.", Icon.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(NumeroCompte))
        {
            await AfficherPopupAsync("Attention", "Veuillez entrer le numéro de compte.", Icon.Warning);
            return;
        }

        if (Montant <= 0)
        {
            await AfficherPopupAsync("Attention", "Le montant doit être supérieur à zéro.", Icon.Warning);
            return;
        }

        // Appel à la méthode du modèle
        Result res = await CreditVirement.PayerCreditAsync(
            numero: NumeroCompte,
            codeAgence: Agence.CodeAgence,
            montant: Montant
        );

        if (res.Status)
        {
            await AfficherPopupAsync("Succès", res.Message, Icon.Success);
            RéinitialiserChamps();
            _headerViewModel.Transaction();
        }
        else
        {
            await AfficherPopupAsync("Échec", res.Message, Icon.Error);
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.Transaction();
    }

    private static async Task AfficherPopupAsync(string titre, string message, Icon icon)
    {
        var box = MessageBoxManager.GetMessageBoxStandard(titre, message, ButtonEnum.Ok, icon);
        await box.ShowAsync();
    }

    private void RéinitialiserChamps()
    {
        NumeroCompte = string.Empty;
        Montant = 0;
    }
}