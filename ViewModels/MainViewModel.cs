using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    public ViewModelBase currentView { get; set; }
}
