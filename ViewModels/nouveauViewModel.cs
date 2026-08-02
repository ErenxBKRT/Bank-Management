using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class NouveauViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string _quoi;

    public NouveauViewModel (HeaderViewModel headerViewModel,string a)
    {
        _headerViewModel = headerViewModel;
        Quoi=a;
    }

    [RelayCommand]
    private void Annuler()
    {
        if (Quoi == "Employe")
        {
            _headerViewModel.MenuEmploye();
        }
        else if (Quoi == "Client")
        {
            _headerViewModel.MenuClient();
        }
        else if (Quoi == "Agence")
        {
            _headerViewModel.MenuAgence();
        }
    }

    [RelayCommand]
    private void Confirmer()
    {
        if (Quoi == "Client")
        {
            //code qui cree un nouveau client
            _headerViewModel.MenuClient();
        }
        else if (Quoi== "Employe")
        {
            //code qui cree un nouveau employe
            _headerViewModel.MenuEmploye();
        }
        else if (Quoi== "Agence")
        {
            //code qui cree un nouveau agence
            _headerViewModel.MenuAgence();
        }
    }
}

