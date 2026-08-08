using Bankmanaging.Models;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class ConnexionViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Entrez vos identifiants pour continuer.";

    public ConnexionViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private async Task Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Veuillez remplir tous les champs.";
            return;
        }
        bool text = await GestionAgence.LogInAsync(Username, Password);

        if (text)
        {
            StatusMessage = "Connexion réussie !";
            _mainViewModel.OuvrirApplication("E");
        }

        if (Username.Equals("Laza", System.StringComparison.OrdinalIgnoreCase) && Password == "1111")
        {
            _mainViewModel.OuvrirApplication("C");
        }
        else
        {
            StatusMessage = "Identifiants invalides";
        }
    }
}