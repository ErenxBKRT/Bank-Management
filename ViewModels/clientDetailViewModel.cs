using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Compte> Comptes { get; } = [];

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
            var comptes = await Listing.ListCompteAsync(Client.Id);

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
    private async Task Bloquer(Compte compte)
    {
        try
        {
            Result result = await ServiceCompte.LockAsync(!compte.Bloque, numero : compte.Numero);

            if (result.Status)
            {
                // compte.Bloque = !compte.Bloque;
                await LoadComptesAsync();
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
    private void CCompte()
    {
        _headerViewModel.CCompte(Client);
    }
}
