using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bankmanaging.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string recherche="";

    public ObservableCollection<Client> Clients {get; } = new(); 

    public ClientsViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        Clients.Add(new Client{
            Nom = "RAKOTO",
            Prenom = "Nirina",
            Id = 12,
            Contact = "034222485",
            Adresse = "Lot djflkdsfjlkd"
        });
        Clients.Add(new Client{
            Nom= "RABE",
            Prenom = "Zafy",
            Id = 3,
            Adresse = "Lot djflkdsfjlkd",
            Contact = "034222535"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 321,
            Adresse = "Lot djflkdsfjlkd"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 32,
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 432,
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 321,
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 321,
            Contact = "0342225135"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            Id = 432
        });
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
    private void Bloquer(Client client)
    {
        
    }
}

