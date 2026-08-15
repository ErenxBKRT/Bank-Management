using CommunityToolkit.Mvvm.Input;
using Bankmanaging.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class AgenceViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;
    public ObservableCollection<Agence> Agences { get; } = [];

    public AgenceViewModel (HeaderViewModel headerViewModel)
    {
        _headerViewModel = headerViewModel;
            // Le constructeur ne peut pas être async
        _ = LoadAgencesAsync();
        
    }

    private async Task LoadAgencesAsync()
    {
        try
        {
            var agences = await Listing.ListAgenceAsync();

            Agences.Clear();
            foreach (var agence in agences)
            {
                Agences.Add(agence);
            }
        }
        catch (Exception ex)
        {
            //un message d'erreur affiché dans la Vue
            Console.WriteLine($"Error : {ex.Message}");
        }
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
