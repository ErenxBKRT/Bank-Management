using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ClientLogViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ClientLogViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }

    [RelayCommand]
    private void Selectionner()
    {
        _headerViewModel.ActionClient();
    }

    [RelayCommand]
    private void AnnulerLog()
    {
        _headerViewModel.RevenirClient();
    }

}

