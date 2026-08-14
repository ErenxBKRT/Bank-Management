using Bankmanaging.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using System.Threading.Tasks;

namespace Bankmanaging.ViewModels;

public partial class ModifierClientViewModel : ViewModelBase
{
    private readonly HeaderViewModel _headerViewModel;

    public Client Client { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCCommand))]
    private string nouveauNom = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCCommand))]
    private string nouveauPrenom = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCCommand))]
    private string nouvelleAdresse = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ModifierCCommand))]
    private string nouveauContact = string.Empty;

    [ObservableProperty]
    private string messageErreur = string.Empty;

    public ModifierClientViewModel(HeaderViewModel headerViewModel, Client client)
    {
        _headerViewModel = headerViewModel;
        Client = client;

        // Pre remplissage Client
        NouveauNom = client.Nom;
        NouveauPrenom = client.Prenom;
        NouvelleAdresse = client.Adresse;
        NouveauContact = client.Contact;
    }

    // Validation
    private bool CanModifier() =>
        !string.IsNullOrWhiteSpace(NouveauNom) &&
        !string.IsNullOrWhiteSpace(NouvelleAdresse) &&
        !string.IsNullOrWhiteSpace(NouveauContact);

    [RelayCommand(CanExecute = nameof(CanModifier))]
    private async Task ModifierCAsync()
    {
        try
        {
            Console.WriteLine(Client.Id);
            // Transmet Client.Id 
            Result result = await ServiceClient.UpdateAsync(
                Client.Id,
                NouveauNom,
                NouvelleAdresse,
                NouveauContact,
                string.IsNullOrWhiteSpace(NouveauPrenom) ? null : NouveauPrenom
            );

            if (result.Status)
            {
                
                Client.Nom = NouveauNom;
                Client.Prenom = NouveauPrenom;
                Client.Adresse = NouvelleAdresse;
                Client.Contact = NouveauContact;

                //Popup de confirmation
                var box = MessageBoxManager.GetMessageBoxStandard(
                    title: "Succès",
                    text: result.Message,
                    ButtonEnum.Ok,
                    Icon.Success
                );

                //Wait for the user to close the message box before navigating back to the client menu
                await box.ShowAsync();
                _headerViewModel.MenuClient();
            }
            else
            {
                MessageErreur = result.Message;
            }
        }
        catch (Exception ex)
        {
            MessageErreur = $"Une erreur est survenue : {ex.Message}";
        }
    }

    [RelayCommand]
    private void Annuler()
    {
        _headerViewModel.MenuClient();
    }
}