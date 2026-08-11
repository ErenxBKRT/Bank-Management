using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class RembourserViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence Agence{get;} 

    [ObservableProperty]
    private decimal montant = 0;

    [ObservableProperty]
    private string numeroCompte = "";

    public RembourserViewModel(HeaderViewModel headerViewModel,Agence agence)
    {
        _headerViewModel= headerViewModel;
        Agence = agence;
    }

    [RelayCommand]
    private void annuler()
    {
        _headerViewModel.Transaction();
    }
}