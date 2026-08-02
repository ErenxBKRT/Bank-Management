using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class EmployeViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public EmployeViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }

    [RelayCommand]
    private void Creer()
    {
        _headerViewModel.Nouveau("Employe");
    }

    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

