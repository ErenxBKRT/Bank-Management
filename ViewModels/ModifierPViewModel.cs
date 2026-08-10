using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Bankmanaging.ViewModels;

public partial class ModifierPViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string ancienPin = "";

    [ObservableProperty]
    private string nouveauPin = "";

    [ObservableProperty]
    private string confirmationPin = "";

    public ModifierPViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void Modifier()
    {
        //code qui modifie le pin
        _headerViewModel.Retrait();
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.Retrait();
    }
}

