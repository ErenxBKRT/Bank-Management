using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class TransactionViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public TransactionViewModel(HeaderViewModel headerViewModel)
    {
        _headerViewModel=headerViewModel;
    }

    [RelayCommand]
    private void Retour()
    {
        _headerViewModel.MainMenu();
    }

    [RelayCommand]
    private void Virement()
    {
        _headerViewModel.Virement();
    }

    [RelayCommand]
    private void Depot()
    {
        _headerViewModel.Depot();
    }

    [RelayCommand]
    private void Credit()
    {
        _headerViewModel.Credit();
    }
}

