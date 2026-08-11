using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CCompteViewModel : ViewModelBase
{
     private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string pIN;

    [ObservableProperty]
    private string confirmationPIN;

    public CCompteViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void CCompte()
    {
        //code aui cree un compte 
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.MenuClient();
    } 
}

