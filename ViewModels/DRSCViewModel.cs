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
        _headerViewModel.RevenirClient();
    }

}