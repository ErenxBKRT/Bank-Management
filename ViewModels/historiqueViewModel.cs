using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using Bankmanaging.Models;

namespace Bankmanaging.ViewModels;

public partial class HistoriqueViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;
    public ObservableCollection<Transaction> Transactions {get; } = new();

    [ObservableProperty]
    private ViewModelBase _historiqueL;

    public HistoriqueViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
        _historiqueL = new HistoriqueAViewModel();
        Transactions.Add(new Transaction{
            IdTransaction = "T0124",
            Type = "Depot",
            CodeAgence = "Q1200"
        });
        Transactions.Add(new Transaction{
            IdTransaction = "T0123",
            Type = "Credit",
            CodeAgence = "Q1200"
        });
        Transactions.Add(new Transaction{
            IdTransaction = "T1234",
            Type = "Virement",
            CodeAgence = "Q1201"
        });
    }
    
    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }
}

