using System;
using Bankmanaging.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class ActionViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    [ObservableProperty]
    private string _action; 

    [ObservableProperty]
    private double _Somme=0;

    public ActionViewModel (HeaderViewModel headerViewModel,string a)
    {
        _headerViewModel = headerViewModel;
        Action=a;
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.EntrerClient();
    }

    [RelayCommand]
    private void Confirmer()
    {
        //code d'action
        if (Action=="Depot")
        {
            //code depot
        }
        else if (Action=="Retrait")
        {
            //code depot
        }
        else if (Action=="Rembourser")
        {
            //code depot
        }
        else if (Action=="Credit")
        {
            //code depot
        }
        _headerViewModel.EntrerClient();
    }
}