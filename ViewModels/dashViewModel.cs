using Bankmanaging.Models;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DashViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Agence? Agence { get; }

    public DashViewModel (HeaderViewModel headerViewModel, Agence agence)
    {
        _headerViewModel= headerViewModel;
        Agence = agence;
    }

    public DashViewModel(HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
    }


    [RelayCommand]
    private void ActionClient()
    {
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void ActionHistorique()
    {
        _headerViewModel.MenuHistorique();
    }

    [RelayCommand]
    private void Transaction()
    {
        _headerViewModel.Transaction();
    }

    [RelayCommand]
    private void ActionAgence()
    {
        _headerViewModel.MenuAgence();
    }

    [RelayCommand]
    private void Rapport()
    {
        _headerViewModel.Rapport();
    }

}
