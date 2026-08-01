using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string greeting = "Bienvenue dans Cbanque";

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string statusMessage = "Entrez vos identifiants pour continuer.";

    [RelayCommand]
    private void Login()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            StatusMessage = "Veuillez remplir tous les champs.";
            return;
        }

        if (Username.Equals("admin", System.StringComparison.OrdinalIgnoreCase) && Password == "1234")
        {
            StatusMessage = "Connexion réussie !";

            var accueilWindow = new AccueilWindow();
        }
        else
        {
            StatusMessage = "Identifiants invalides. Essayez admin / 1234.";
        }
    }
}