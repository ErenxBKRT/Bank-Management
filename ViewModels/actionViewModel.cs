using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ActionViewModel : ViewModelBase
{
    [ObservableProperty]
    private string action_AFaire;
    public ActionViewModel(string action)
    {
        Action_AFaire = action;
    }
}