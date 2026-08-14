using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;


namespace Bankmanaging.ViewModels;

public partial class CreditViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence? Agence{get;}

    [ObservableProperty]
    private string _numeroCompte = "";

    [ObservableProperty]
    private decimal _somme = 0;

    //pour afficher les erreurs de validation / échec dans la Vue.
    [ObservableProperty]
    private string _statusMessage = "";

    public CreditViewModel(HeaderViewModel headerViewModel,Agence? agence)
    {
        _headerViewModel= headerViewModel;
        Agence = agence;
    }

    [RelayCommand]
    private async Task Valider()
    {
        if (string.IsNullOrWhiteSpace(NumeroCompte))
        {
            StatusMessage = "Le numéro de compte est obligatoire.";
            return;
        }

        if (Somme <= 0)
        {
            StatusMessage = "Le montant doit être positif.";
            return;
        }

        try
        {
            // Elle vérifie en interne : existence du compte, existence de l'agence, et si le compte appartient à l'agence.
            Result result = await CreditVirement.CreditAsync(NumeroCompte, Agence.CodeAgence, Somme);

            if (result.Status)
            {
                _headerViewModel.Transaction();
            }
            else
            {
                StatusMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            StatusMessage = "Une erreur est survenue lors du crédit.";
        }
    }

    [RelayCommand]
    private void annuler()
    {
        _headerViewModel.Transaction();
    }
}
