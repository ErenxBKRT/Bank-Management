using System;
using Bankmanaging.Models;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

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
            Result task = await ServiceClient.AddAsync("Doe", "STREET", "0328091283", "Jane");
            StatusMessage = task.Message;
        }

        try
        {
            Result logged = await GestionAgence.LogInAsync(Username, Password);
            if (!logged.Status)
            {
                StatusMessage = logged.Message;
                return;
            }
            _mainViewModel.OuvrirApplication("E");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            StatusMessage = "Please report to dev for this error!";
            return;
        }
        
        try 
        {
            Result logged = await ServiceCompte.LogInAsync(Username, Password);
            if (!logged.Status)
            {
                StatusMessage = logged.Message;
                return;                
            }
            _mainViewModel.OuvrirApplication("C");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            StatusMessage = "Please report to dev for this error!";
            return;
        }
    }
}