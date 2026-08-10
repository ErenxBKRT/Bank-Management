using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Carte> Cartes {get; } = new();

    public Client Client {get;}
    public ClientDetailViewModel (HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client=client;
        Cartes.Add(new Carte
        {
           Numero = "Ca123",
           Nom = "lazer",
           Prenom = "GG",
           solde = 12000000
        });

        Cartes.Add(new Carte
        {
           Numero = "Ca123",
           Nom = "ErenxBKRT",
           Prenom = "GG",
           solde = 120000
        });

        Cartes.Add(new Carte
        {
           Numero = "Ca123",
           Nom = "Hala",
           Prenom = "Wakubar",
           solde = 15000000
        });

        Cartes.Add(new Carte
        {
           Numero = "Ca123",
           Nom = "Hallo",
           Prenom = "Wak",
           solde = 15000000
        });
    }

    [RelayCommand]
    private void Retour()
    {
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void Modifier()
    {
        
    }

    [RelayCommand]
    private void Bloquer()
    {
        
    }

    [RelayCommand]
    private void Virement()
    {
        
    }

    [RelayCommand]
    private void Depot()
    {
        
    }

    [RelayCommand]
    private void Credit()
    {
        
    }

    [RelayCommand]
    private void CCompte()
    {
        _headerViewModel.CCompte();
    }
}