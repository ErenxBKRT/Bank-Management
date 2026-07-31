using Npgsql;

namespace Bankmanaging.Models;

class AddClient
{
    public required string IdClient;
    public required string Nom;
    public string? Prenom;
    public required string Adresse;
    public string? Mail;
    public string? Contact;
    public required decimal Solde;
    public required string Pin;
    public decimal? dette;

    public async InsertIntoClient(IDatabaseConnection kaeru)
    {
        const string query = "INSERT INTO (id_client, nom, prenom, adresse, mail, contact, solde, pin, dette) VALUES (@id, @nom, @prenom, @adresse, @mail, @contact, @solde, @pin, @dette)"
    }
}
