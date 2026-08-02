using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class AgenceViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public AgenceViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }

    [RelayCommand]
    private void Creer()
    {
        _headerViewModel.Nouveau("Agence");
    }
    
    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

