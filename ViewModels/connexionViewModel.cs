using System;
using Bankmanaging.Models;
using Bankmanaging.Services;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Diagnostics;

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
            StatusMessage = "Veuillez remplir tous les champs";
            return;
        }
        try
        {
            LoginAccountAgence logAsEmploye = await GestionAgence.LogInAsync(Username, Password);
            LoginAccountClient logAsClient = await ServiceCompte.LogInAsync(Username, Password);

            if (!logAsEmploye.Status)
            {
                if (!logAsClient.Status)
                {
                    if (logAsClient.Message == "Le compte est bloqué")
                    {
                        StatusMessage = logAsClient.Message;
                    }
                    else StatusMessage = logAsEmploye.Message;
                }
                else if (logAsClient.Status)
                {
                    //inject session info
                    SessionServ.StartClientSession(logAsClient.CompteClient!);
                    _mainViewModel.OuvrirApplication("C", compte: logAsClient.CompteClient);
                }
            }
            else if (logAsEmploye.Status)
            {
                SessionServ.StartEmployeSession(logAsEmploye.CompteAgence!);
                _mainViewModel.OuvrirApplication("E", agence: logAsEmploye.CompteAgence);
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