using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class ClientsViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Client> Clients {get; } = new(); 

    public ClientsViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        Clients.Add(new Client{
            Nom = "RAKOTO",
            Prenom = "Nirina",
            IdClient = "C1234",
            Contact = "034222485",
            Adresse = "Lot djflkdsfjlkd"
        });
        Clients.Add(new Client{
            Nom= "RABE",
            Prenom = "Zafy",
            IdClient = "C4321",
            Adresse = "Lot djflkdsfjlkd",
            Contact = "034222535"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321",
            Adresse = "Lot djflkdsfjlkd"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321",
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321",
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321",
            Contact = "0342225245"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321",
            Contact = "0342225135"
        });
        Clients.Add(new Client{
            Nom= "RAZAFY",
            Prenom = "Koto",
            IdClient = "C4321"
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
}

