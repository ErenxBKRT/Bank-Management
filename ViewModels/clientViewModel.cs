using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ClientViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ClientViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }

    [RelayCommand]
    private void Connecter()
    {
        _headerViewModel.ConnecterClient();
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
    private void Nouveau()
    {
        _headerViewModel.Nouveau("Client");
    }

}

