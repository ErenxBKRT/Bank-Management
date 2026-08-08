using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using System.Collections.ObjectModel;

namespace Bankmanaging.ViewModels;

public partial class AgenceViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;
     public ObservableCollection<Agence> Agences {get; } = new();

    public AgenceViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        Agences.Add(new Agence
        {
            CodeAgence="A1234",
            Adresse = "lot Tanambao",
            Solde = 1200000
        });
        Agences.Add(new Agence{
            CodeAgence = "a4321",
            Adresse = "lot Andrainjato",
            Solde = 210000000
        });
    }

    [RelayCommand]
    private void CreationA()
    {
        _headerViewModel.NouveauA();
    }
    
    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

