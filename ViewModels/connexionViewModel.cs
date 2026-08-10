using System;
using Bankmanaging.Models;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace Bankmanaging.ViewModels;

public partial class ConnexionViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _statusMessage = "Entrez vos identifiants pour continuer.";

    public ConnexionViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            IEnumerable<Compte> comptes = await Listing.ListCompteCreditAsync();
            foreach (Compte compte in comptes)
            {
                Console.WriteLine($"{compte.Numero}, {compte.Solde}, {compte.Credit}, {compte.Bloque}");
            }

            StatusMessage = "Veuillez remplir tous les champs";
            return;
        }
        try
        {
            Result logAsEmploye = await GestionAgence.LogInAsync(Username, Password);
            Result logAsClient = await ServiceCompte.LogInAsync(Username, Password);

            if (!logAsEmploye.Status)
            {
                if (!logAsClient.Status)
                {
                    if(logAsClient.Message == "Le compte est bloqué")
                    {
                        StatusMessage = logAsClient.Message;
                    }
                    else StatusMessage = logAsEmploye.Message;
                }
                else if (logAsClient.Status) _mainViewModel.OuvrirApplication("C");
            }
            else if (logAsEmploye.Status)
            {
                _mainViewModel.OuvrirApplication("E");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            StatusMessage = "Please report to dev for this error!";
            return;
        }
    }
}