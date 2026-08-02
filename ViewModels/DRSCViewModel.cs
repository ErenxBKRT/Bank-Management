using System.Net.Http.Headers;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DRSCViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public DRSCViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void RetourClient()
    {
        _headerViewModel.MenuClient();
    }
    [RelayCommand]
    private void Depot()
    {
        _headerViewModel.ActionClient("Depot");
    }
    [RelayCommand]
    private void Retrait()
    {
        _headerViewModel.ActionClient("Retrait");
    }

    [RelayCommand]
    private void Credit()
    {
        _headerViewModel.ActionClient("Credit");
    }

    [RelayCommand]
    private void Rembourser()
    {
        _headerViewModel.ActionClient("Rembourser");
    }
}