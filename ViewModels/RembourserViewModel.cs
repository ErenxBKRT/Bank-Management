using System.Net.Http.Headers;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class RembourserViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public RembourserViewModel(HeaderViewModel headerViewModel)
    {
        _headerViewModel= headerViewModel;
    }

    [RelayCommand]
    private void annuler()
    {
        _headerViewModel.Transaction();
    }
}