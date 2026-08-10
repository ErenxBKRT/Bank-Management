using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Compte> Comptes {get; } = new();

    public Client Client {get;}
    public ClientDetailViewModel (HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client=client;
        Comptes.Add(new Compte
        {
           Numero = "Ca123",
           Solde = 12000000,
           Credit = 1000000,
           Bloque = true
        });

        Comptes.Add(new Compte
        {
           Numero = "Ca123",
           Solde = 12000000,
           Credit = 1000000,
           Bloque = true
        });

        Comptes.Add(new Compte
        {
           Numero = "Ca123",
           Solde = 12000000,
           Credit = 1000000,
           Bloque = true
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
}