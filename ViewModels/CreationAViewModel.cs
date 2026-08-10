using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class CreationAViewModel : ViewModelBase
{
     private readonly HeaderViewModel _headerViewModel;

    public CreationAViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void CreationA()
    {
        //code aui cree un agence 
        _headerViewModel.MenuAgence();
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.MenuAgence();
    } 
}

