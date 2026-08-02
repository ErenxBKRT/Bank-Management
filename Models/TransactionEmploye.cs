using Npgsql;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Bankmanaging.Models;

public class AddEmploye (string id, string nom, string password, string codeAgence, string prenom = "")
{
    public string IdEmploye { get; set; } = id;
    public string Nom { get; set; } = nom;
    public string Prenom { get; set; } = prenom;
    public string Password { get; set; } = password;
    public string CodeAgence { get; set; } = codeAgence;

    public async Task<string> InsertIntoEmploye (IDatabaseConnection kaeru)
    {
        const string query = "INSERT INTO employe (id_employe, nom, prenom, passwords, code_agence) VALUES (@id, @nom, @prenom, @password, @code);";
        using var conn = kaeru.Connected();
        await conn.OpenAsync();
        using var prepareQuery = new NpgsqlCommand(query, conn);
        prepareQuery.Parameters.AddWithValue("id", IdEmploye);
        prepareQuery.Parameters.AddWithValue("nom", Nom);
        prepareQuery.Parameters.AddWithValue("prenom", Prenom);
        prepareQuery.Parameters.AddWithValue("password", Password);
        prepareQuery.Parameters.AddWithValue("code", CodeAgence);
        try {
            prepareQuery.ExecuteNonQuery();
            return "Success";
        } catch (NpgsqlException ex) {
            Debug.WriteLine(ex); //temporary
            return $"Error";
        }
    }

}
