using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CreationCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string nom = "";

    [ObservableProperty]
    private string prenom = "";

    [ObservableProperty]
    private string adresse = "";

    [ObservableProperty]
    private string contact = "";

    public CreationCViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void CreationC()
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

