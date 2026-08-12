using Bankmanaging.Models;
using Bankmanaging.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class HistoriqueViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Transaction> Transactions { get; } = new();

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _messageErreur = string.Empty;

    public HistoriqueViewModel(HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;

        // Charger les données de manière asynchrone au démarrage
        _ = ChargerHistoriqueAsync();
    }

    [RelayCommand]
    private async Task ChargerHistoriqueAsync()
    {
        IsLoading = true;
        MessageErreur = string.Empty;
        Transactions.Clear();

        try
        {
            IEnumerable<Transaction> resultats;

  
            // Backup : Récupère tout si aucune restriction
            resultats = await Listing.HistoriqueTransactionAsync();

            foreach (var item in resultats)
            {
                Transactions.Add(item);
                Console.WriteLine(item.Numero);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    private void Menu()
    {
        _headerViewModel.MainMenu();
    }

    [RelayCommand]
    private async Task GenPdf() { 
    }
}