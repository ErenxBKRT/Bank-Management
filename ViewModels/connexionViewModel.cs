using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ConnexionViewModel : ViewModelBase
{
    private readonly MainViewModel _mainViewModel;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;
    private string statusMessage = "Entrez vos identifiants pour continuer.";

    public string GetStatusMessage()
    {
    }

    public void SetStatusMessage(string value)
    {
    }

    public ConnexionViewModel(MainViewModel mainViewModel)
    {
        _mainViewModel = mainViewModel;
    }

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            SetStatusMessage("Veuillez remplir tous les champs.");
            return;
        }

        if (Username.Equals("admin", System.StringComparison.OrdinalIgnoreCase) && Password == "1234")
        {
            SetStatusMessage("Connexion réussie !");
            _mainViewModel.OuvrirApplication("E");
        }

        if (Username.Equals("admin", System.StringComparison.OrdinalIgnoreCase) && Password == "1234")
        {
            SetStatusMessage("");
            _mainViewModel.OuvrirApplication("C");
        }
        else
        {
            SetStatusMessage("");
        }
    }
}