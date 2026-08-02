using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class HistoriqueViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public HistoriqueViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }
    
    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

