using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using Bankmanaging.Models;
using CommunityToolkit.Mvvm.Input;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
namespace Bankmanaging.ViewModels;

public partial class ClientDetailViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public ObservableCollection<Compte> Comptes { get; } = [];

    public Client Client { get; }

    public ClientDetailViewModel(HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client = client;
        _ = LoadComptesAsync();
    }

    private async Task LoadComptesAsync()
    {
        try
        {
            var comptes = await Listing.ListCompteAsync(Client.Id);

            Comptes.Clear();
            foreach (var compte in comptes)
            {
                Comptes.Add(compte);
            }
        }
        catch (Exception ex)
        {
        
            Console.WriteLine($"Error : {ex.Message}");
        }
    }

    [RelayCommand]
    private void Retour()
    {
        _headerViewModel.MenuClient();
    }

    [RelayCommand]
    private void Modifier()
    {
        _headerViewModel.ModifierClient(Client);
    }

    [RelayCommand]
    private async Task Bloquer(Compte compte)
    {
        try
        {
            Result result = await ServiceCompte.LockAsync(!compte.Bloque, numero : compte.Numero);

            if (result.Status)
            {
                // compte.Bloque = !compte.Bloque;
                await LoadComptesAsync();
            }
            else
            {
                Console.WriteLine($"Blocage échoué : {result.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
        }
    }

    [RelayCommand]
    private void CCompte()
    {
        _headerViewModel.CCompte(Client);
    }

    private async Task<byte[]> GenPdf()
    {
        var transactionsParCompte = new Dictionary<string, IEnumerable<Transaction>>();
        foreach (var compte in Comptes)
        {
            try
            {
                transactionsParCompte[compte.Numero] = await Listing.HistoriqueTransactionAsync(numero: compte.Numero);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur récupération historique pour le compte {compte.Numero} : {ex.Message}");
                transactionsParCompte[compte.Numero] = [];
            }
        }

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .Text("Bankkun")
                    .SemiBold().FontSize(24)
                    .FontColor(Colors.Blue.Medium);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(col =>
                    {
                        col.Item().Text($"Client : {Client.Nom} {Client.Prenom}").FontSize(16);
                        col.Item().PaddingTop(10);

                        foreach (var compte in Comptes)
                        {
                            col.Item().Text($"Compte {compte.Numero} — Solde : {compte.Solde}");
                            col.Item().PaddingTop(5);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(5);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Code").SemiBold();
                                    header.Cell().Text("Libellé").SemiBold();
                                    header.Cell().Text("Montant").SemiBold();
                                    header.Cell().Text("Date").SemiBold();
                                    header.Cell().Text("Description").SemiBold();

                                    header.Cell().ColumnSpan(5).PaddingTop(2).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                                });

                                foreach (var transaction in transactionsParCompte[compte.Numero])
                                {
                                    table.Cell().Text(transaction.Code);
                                    table.Cell().Text(transaction.Libelle);
                                    table.Cell().Text($"{transaction.Montant:N2}");
                                    table.Cell().Text(transaction.Date.ToString("dd/MM/yyyy HH:mm"));
                                    table.Cell().Text(transaction.Description ?? "-");
                                }
                            });

                            col.Item().PaddingTop(15);
                        }
                    });

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
    // Écrit le PDF dans le flux fourni
    public async Task DownPdfAsync(Stream streamTarget)
    {
        if (streamTarget is null) return;

        byte[] pdfBytes = await GenPdf();
        await streamTarget.WriteAsync(pdfBytes);
    }

    // Ouvre le dialogue de sauvegarde
    [RelayCommand]
    public async Task DownPdf(Compte compte)
    {
        var topLevel = GetTopLevel();
        if (topLevel?.StorageProvider is null) return;

        var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = $"Enregistrer le relevé bancaire {compte.Numero}",
            DefaultExtension = "pdf",
            SuggestedFileName = $"Releve_{Client.Nom}.pdf",
            FileTypeChoices = new[]
            {
            new FilePickerFileType("Fichiers PDF (*.pdf)")
            {
                Patterns = new[] { "*.pdf" },
                MimeTypes = new[] { "application/pdf" }
            }
        }
        });

        if (file is null) return;

        try
        {
            await using var stream = await file.OpenWriteAsync();
            await DownPdfAsync(stream);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erreur génération PDF : {ex}");
        }
    }
    private static TopLevel? GetTopLevel()
    {
        if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow;
        }

        if (Avalonia.Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime single)
        {
            return TopLevel.GetTopLevel(single.MainView);
        }

        return null;
    }
}