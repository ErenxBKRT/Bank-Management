using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class VirementViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence Agence {get;}

    [ObservableProperty]
    private string numeroCompte = "";

    [ObservableProperty]
    private decimal montant = 0;
    
    [ObservableProperty]
    private string description = "";

    public VirementViewModel(HeaderViewModel headerViewModel, Agence agence)
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