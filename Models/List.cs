using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bankmanaging.Models;

public static class ManageTransaction
{
    public static async Task<IEnumerable<Client>> GetClientListAsync (int? idClient = null, bool? bloque = null, string? nom = null)
    {
        List<Client> listClient = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("SELECT * FROM client WHERE (id_client = @idClient OR @idClient IS NULL) AND (bloque = @bloque OR @bloque IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
        preparedQuery.Parameters.AddWithValue("bloque", bloque ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("idClient", idClient ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);
        
        try 
        {
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    Id = row.GetInt32(0),
                    Nom = row.GetString(1),
                    Prenom = row.IsDBNull(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Contact = row.GetString(4),
                    Bloque = row.GetBoolean(6)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    // ~ HISTORIQUE TRANSACTION
    public static async Task<IEnumerable<Transaction>> HistoriqueTransactionAsync(string? numero = null, string? libelle = null, string? codeAgence = null)
    {
        List<Transaction> listTransaction = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("SELECT * FROM transaction WHERE (num_compte = @numero OR @numero IS NULL) AND (libelle = @libelle OR @libelle IS NULL) AND (code_agence = @codeAgence OR @codeAgence is NULL));", kaeru);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("libelle", libelle ?? (object)DBNull.Value);
        try
        {
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Transaction transaction = new()
                {
                    Code = row.GetString(0),
                    Libelle = row.GetString(1),
                    Montant = row.GetDecimal(2),
                    Status = row.GetString(3),
                    Date = row.GetDateTime(4),
                    Nom = await row.IsDBNullAsync(5) ? null : row.GetString(5),
                    CodeAgence = await row.IsDBNullAsync(6) ? null : row.GetString(6),
                    Numero = row.GetString(7)
                };
                listTransaction.Add(transaction);
            }
            return listTransaction;
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    // ~ LIST CLIENT AYANT CREDIT
    public static async Task<IEnumerable<Client>> ListClientCreditAsync (string? nom = null)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("SELECT nom, prenom, adresse, contact, credit, date FROM client WHERE credit > 0.00 AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
        preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);

        List<Client> listClient = [];
        try
        {
            NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    Nom = row.GetString(0),
                    Prenom = await row.IsDBNullAsync(1) ? null : row.GetString(1),
                    Adresse = row.GetString(2),
                    Contact = row.GetString(3),
                    Credit = row.GetDecimal(4)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    public static async Task<List<Carte>> ListCarteAsync (string? numero = null, string? nom = null)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("SELECT num_compte, nom, prenom FROM carte_bancaire WHERE (num_compte = @numero OR @numero IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
        preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);
        List<Carte> listeCarte = [];

        try
        {
            NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Carte carte = new()
                {
                    Numero = row.GetString(0),
                    Nom = row.GetString(1),
                    Prenom = row.GetString(2)
                };
                listeCarte.Add(carte);
            }
            return listeCarte;
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

}
