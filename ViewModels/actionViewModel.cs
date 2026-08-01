using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ActionViewModel : ViewModelBase
{

    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string action_AFaire;
    public ActionViewModel(string action,HeaderViewModel headerViewModel)
    {
        Action_AFaire = action;
        _headerViewModel = headerViewModel;
    }

    [RelayCommand]
    private void AnnulerAction()
    {
        _headerViewModel.AnnulerAction();
    }
}