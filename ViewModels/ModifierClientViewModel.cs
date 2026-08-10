using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class ModifierClientViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;
    
    public Client Client { get; }

    public ModifierClientViewModel (HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel= headerViewModel;
        Client = client;
    }

    [RelayCommand]
    private void ModifierC()
    {
        //code aui cree une client
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.MenuClient();
    }
}

