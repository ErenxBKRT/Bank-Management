using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Client Client {get;}
    public ClientDetailViewModel (HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client=client;
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