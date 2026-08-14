using System;
using System.Threading.Tasks;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CreationCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string _nom = "";

    [ObservableProperty]
    private string _prenom = "";

    [ObservableProperty]
    private string _adresse = "";

    [ObservableProperty]
    private string _contact = "";

    public CreationCViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }
    
    //pour afficher les erreurs de validation / échec dans la Vue
    [ObservableProperty]
    private string _statusMessage = "";

    [RelayCommand]
    private async Task CreationC()
    {
        if (string.IsNullOrWhiteSpace(Nom))
        {
            StatusMessage = "Le nom est obligatoire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Prenom))
        {
            StatusMessage = "Le prénom est obligatoire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Adresse))
        {
            StatusMessage = "L'adresse est obligatoire.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Contact))
        {
            StatusMessage = "Le contact est obligatoire.";
            return;
        }

        try
        {
            Result result = await ServiceClient.AddAsync(Nom, Prenom, Adresse, Contact);

            if (result.Status)
            {
                _headerViewModel.MenuClient();
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
        _headerViewModel.MenuClient();
    }
}

