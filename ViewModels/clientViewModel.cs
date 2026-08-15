using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bankmanaging.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string recherche="";

    public ObservableCollection<Client> Clients {get; } = []; 

    public ClientsViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        _ = LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            // Formatage du paramètre pour la recherche SQL avec ILIKE / LIKE (%)
            string? filtreNom = string.IsNullOrWhiteSpace(Recherche) ? null : $"%{Recherche.Trim()}%";

            // Envoi filtre to fonction de base de données
            var clients = await Listing.ListClientAsync(nom: filtreNom);

            Clients.Clear();
            foreach (var client in clients)
            {
                Clients.Add(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
        }
    }

    [RelayCommand]
    private void Selectionner()
    {
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void MenuP()
    {
        _headerViewModel.MainMenu();
    }

    [RelayCommand]
    private void CreationC()
    {
        _headerViewModel.NouveauC();
    }

   [RelayCommand]
    private void Acceder(Client client)
    {
        _headerViewModel.OuvrirClient(client);
    }

    [RelayCommand]
    private void Modifier(Client client)
    {
        _headerViewModel.ModifierClient(client);
    }

    [RelayCommand]
    private async Task Bloquer(Client client)
    {
        try
        {
            Result result = await ServiceClient.LockAsync(!client.Bloque, client.Id);

            if (result.Status)
            {
                client.Bloque = !client.Bloque;
                // recharger la liste entière après un blocage réussi.
                await LoadClientsAsync();
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

    //recherche
    async partial void OnRechercheChanged(string value)
    {
        await LoadClientsAsync();
    }
}
