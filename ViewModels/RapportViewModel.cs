using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Bankmanaging.ViewModels;

public partial class RapportViewModel : ViewModelBase
{
     private readonly HeaderViewModel _headerViewModel;

     public Agence Agence {get;}

    public RapportViewModel (HeaderViewModel headerViewModel,Agence agence)
    {
        _headerViewModel= headerViewModel;
        Agence = agence;
    }

    [RelayCommand]
    public void Retour()
    {
        _headerViewModel.MainMenu();
    }
}
