using CommunityToolkit.Mvvm.ComponentModel;

namespace Bankmanaging.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase currentView;

    public MainViewModel()
    {
        CurrentView = new ConnexionViewModel(this);
    }

    public void OuvrirApplication(string mode, Models.Agence? agence = null, Models.Compte? compte = null)
    {
        if (agence == null)
        {
            CurrentView = new HeaderViewModel(this, mode, compte: compte);
        }
        else if (compte == null)
        {
            CurrentView = new HeaderViewModel(this, mode, agence: agence);
        }
    }

    public void Deco()
    {
        CurrentView = new ConnexionViewModel(this);
    }

}