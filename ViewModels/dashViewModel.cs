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
    private void Action(string e)
    {
        // Change the current view to ClientListViewModel
        if (e == "Depot")
        {
            _headerViewModel.ChangerAction("depot");
        }
        else if (e == "Retrait")
        {
            _headerViewModel.ChangerAction("retrait");
        }
        else if (e == "Virement")
        {
            _headerViewModel.ChangerAction("virement");
        }
        else if (e == "Credit")
        {
            _headerViewModel.ChangerAction("credit");
        }
        else if (e == "Client")
        {                   
            _headerViewModel.ChangerAction("client");
        }
        else if (e == "Transaction")
        {
            _headerViewModel.ChangerAction("transaction");
        }
    }

}