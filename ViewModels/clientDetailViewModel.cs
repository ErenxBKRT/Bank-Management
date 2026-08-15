using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Compte> Comptes { get; } = new();

    public Client Client { get; }

    public ClientDetailViewModel(HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client = client;

        _ = LoadComptesAsync();
    }

    private async Task LoadComptesAsync()
    {
        try
        {
            var comptes = await Listing.ListCompteAsync(Client.Id.ToString());

            Comptes.Clear();
            foreach (var compte in comptes)
            {
                Comptes.Add(compte);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
        }
    }

    [RelayCommand]
    private void Retour()
    {
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void Modifier()
    {
        _headerViewModel.ModifierClient(Client);
    }

    [RelayCommand]
    private async Task Bloquer()
    {
        try
        {
            Result result = await ServiceClient.LockAsync(!Client.Bloque, Client.Id);

            if (result.Status)
            {
                Client.Bloque = !Client.Bloque;
            }
            else
            {
                Console.WriteLine($"Blocage échoué : {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
        }
    }

    [RelayCommand]
    private void Virement()
    {
        _headerViewModel.Virement();
    }

    [RelayCommand]
    private void Depot()
    {
        _headerViewModel.Depot();
    }

    [RelayCommand]
    private void Credit()
    {
        _headerViewModel.Credit();
    }

    [RelayCommand]
    private void CCompte()
    {
        _headerViewModel.CCompte(Client);
    }
}
