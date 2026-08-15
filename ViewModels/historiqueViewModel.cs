using Avalonia.Platform.Storage;
using Bankmanaging.Models;
using Bankmanaging.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
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


    private byte[] GenPdf() {
        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(20));
                
                page.Header()
                    .Text("Bankkun")
                    .SemiBold().FontSize(24)
                    .FontColor(Colors.Blue.Medium);

                page.Content()
                .PaddingVertical(1, Unit.Centimetre)
                .Text("This is shit");

                page.Footer()
                    .AlignCenter()
                    .Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
            });

        });
        return doc.GeneratePdf();
    }

    [RelayCommand]
    public async Task DownPdf(IStorageProvider storageProvider)
    {
        // 1. Demander à l'utilisateur où enregistrer le fichier
        var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Enregistrer le PDF",
            DefaultExtension = "pdf",
            SuggestedFileName = "HelloWorld.pdf",
            FileTypeChoices = new[]
            {
                new FilePickerFileType("Fichiers PDF (*.pdf)")
                {
                    Patterns = new[] { "*.pdf" },
                    MimeTypes = new[] { "application/pdf" }
                }
            }
        });

        if (file is null) return; // L'utilisateur a annulé

        // 2. Générer les octets du PDF "Hello World"
        byte[] pdfBytes = GenPdf();

        // 3. Écrire le contenu dans le fichier choisi
        await using var stream = await file.OpenWriteAsync();
        await stream.WriteAsync(pdfBytes);
    }

}