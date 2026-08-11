using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class HistoriqueViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;
    public ObservableCollection<Transaction> Transactions {get; } = new();

    

    public HistoriqueViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        Transactions.Add(new Transaction{
            Code = "T0124",
            Libelle = "Depot",
            CodeAgence = "Q1200"
        });
        Transactions.Add(new Transaction{
            Code = "T0123",
            Libelle = "Credit",
            CodeAgence = "Q1200"
        });
        Transactions.Add(new Transaction{
            Code = "T1234",
            Libelle = "Virement",
            CodeAgence = "Q1201"
        });
    }
    
    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

