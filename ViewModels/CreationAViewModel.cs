using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CreationAViewModel : ViewModelBase
{
     private readonly HeaderViewModel _headerViewModel;

     [ObservableProperty]
     private string _adresse = "";

     [ObservableProperty]
     private decimal _solde = 0;

     [ObservableProperty]
     private string _PIN = "";

     [ObservableProperty]
     private string _confirmationPIN = "";

     //pour afficher les erreurs de validation / échec dans la Vue
     [ObservableProperty]
     private string _statusMessage = "";

    public CreationAViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private async Task CreationA()
    {

        if (string.IsNullOrWhiteSpace(Adresse))
        {
            StatusMessage = "L'adresse est obligatoire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(PIN) || string.IsNullOrWhiteSpace(ConfirmationPIN))
        {
            StatusMessage = "Veuillez saisir et confirmer le PIN.";
            return;
        }

        if (PIN != ConfirmationPIN)
        {
            StatusMessage = "Le PIN et sa confirmation ne correspondent pas.";
            return;
        }

        if (Solde < 0)
        {
            StatusMessage = "Le solde initial ne peut pas être négatif.";
            return;
        }

        try
        {
            Result result = await GestionAgence.AddAsync(Adresse, Solde, PIN);

            if (result.Status)
            {
                _headerViewModel.MenuAgence();
            }
            else
            {
                StatusMessage = result.Message;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            StatusMessage = "Une erreur est survenue lors de la création.";
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.MenuAgence();
    } 
}
