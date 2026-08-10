using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bankmanaging.ViewModels;

public partial class ModifierClientViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string nouveauNom = "";

    [ObservableProperty]
    private string nouveauPrenom = "";

    [ObservableProperty]
    private string nouvelleAdresse = "";

    [ObservableProperty]
    private string nouveauContact = "";

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

