using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CreationCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

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

