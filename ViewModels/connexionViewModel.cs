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
        Result random = await GestionAgence.LogInAsync(Username, Password);

        if (random.Status)
        {
            StatusMessage = random.Message ?? "random not";
            _mainViewModel.OuvrirApplication();
        }
        else
        {
            StatusMessage = random.Message ?? "random";
        }
    }
}