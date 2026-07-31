using System.Threading.Tasks;
using Npgsql;

namespace Bankmanaging.Models;

class AddClient
{
    public string IdClient;
    public string Nom;
    public string Prenom;
    public string Adresse;
    public string Mail;
    public string Contact;
    public decimal Solde;
    public string Pin;
    public decimal Dette;

    public AddClient (string id, string nom, string adresse, string pin, decimal dette = 0.00m, string prenom = "", decimal solde = 0.00m, string contact = "No contact", string mail = "No mail")
    {
        IdClient = id;
        Nom = nom;
        Prenom = prenom;
        Adresse = adresse;
        Mail = mail;
        Contact = contact;
        Solde = solde;
        Pin = pin;
        Dette = dette;
    }

    public async Task InsertIntoClient(IDatabaseConnection kaeru)
    {
        const string query = "INSERT INTO (id_client, nom, prenom, adresse, mail, contact, solde, pin, dette) VALUES (@id, @nom, @prenom, @adresse, @mail, @contact, @solde, @pin, @dette);";
        using var conn = kaeru.Connected();
        await conn.OpenAsync();
        using var prepareQuery = new NpgsqlCommand(query, conn);
        prepareQuery.Parameters.AddWithValue("id", IdClient);
        prepareQuery.Parameters.AddWithValue("nom", Nom);
        prepareQuery.Parameters.AddWithValue("prenom", Prenom);
        prepareQuery.Parameters.AddWithValue("adresse", Adresse);
        prepareQuery.Parameters.AddWithValue("mail", Mail);
        prepareQuery.Parameters.AddWithValue("contact", Contact);
        prepareQuery.Parameters.AddWithValue("solde", Solde);
        prepareQuery.Parameters.AddWithValue("pin", Pin);
        prepareQuery.Parameters.AddWithValue("dette", Dette);
    }
}
