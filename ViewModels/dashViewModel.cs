using System.Net.Http.Headers;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class DashViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public DashViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void ActionClient()
    {
        _headerViewModel.RevenirClient();
    }
    [RelayCommand]
    private void ActionEmployer()
    {
        _headerViewModel.RevenirClient();
    }
    [RelayCommand]
    private void ActionAgence()
    {
        _headerViewModel.RevenirClient();
    }
    [RelayCommand]
    private void ActionHistorique()
    {
        _headerViewModel.RevenirClient();
    }

}