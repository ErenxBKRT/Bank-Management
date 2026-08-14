using System.Threading.Tasks;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Tmds.DBus.Protocol;

namespace Bankmanaging.ViewModels;

public partial class CCompteViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    private readonly Client _client;

    [ObservableProperty]
<<<<<<< HEAD
    private string pIN = string.Empty;

    [ObservableProperty]
    private string confirmationPIN = string.Empty;
=======
    private string _PIN = string.Empty;

    [ObservableProperty]
    private string _confirmationPIN = string.Empty;
>>>>>>> origin/nate

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private bool _isBusy; // isBusy est utilisé pour désactiver les boutons pendant l'exécution d'une commande asynchrone.

    public CCompteViewModel(HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        _client = client;
    }

    [RelayCommand]
    private async Task CCompteAsync()
    {
        if (IsBusy) return;

        if (string.IsNullOrWhiteSpace(PIN) || PIN != ConfirmationPIN)
        {
            Message = "Les deux PIN doivent être identiques.";
            return;
        }

        IsBusy = true;
        try
        {
            Result result = await ServiceCompte.CreateAsync(_client.Id, PIN);
            Message = result.Message;

            if (result.Status)
            {
                PIN = string.Empty;
                ConfirmationPIN = string.Empty;
                _headerViewModel.MenuClient();
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        PIN = string.Empty;
        ConfirmationPIN = string.Empty;
        Message = string.Empty;
        _headerViewModel.MenuClient();
    }
}
