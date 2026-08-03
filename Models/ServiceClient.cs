using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Bankmanaging.Models;
public static class ServiceClient
{
    // ~INSERT THE NEW CLIENT INTO THE DATABASE 
    public static async Task<string> CreateClientAsync (Client client, string pin)
    {
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 1000);
        client.IdClient = "C" + microSecond + randomNumber.ToString("D3");

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = @"INSERT INTO clients (id_client, nom, prenom, adresse, mail, contact, solde, pin)
                            VALUES (@id, @nom, @prenom, @adresse, @mail, @contact, @solde, @pin);";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("id", client.IdClient);
        preparedQuery.Parameters.AddWithValue("nom", client.Nom);
        preparedQuery.Parameters.AddWithValue("prenom", (object?)client.Prenom ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("adresse", client.Adresse);
        preparedQuery.Parameters.AddWithValue("mail", (object?)client.Mail ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("contact", (object?)client.Contact ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("solde", client.Solde);
        preparedQuery.Parameters.AddWithValue("pin", pin);

        try 
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "success";
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return "error";
        }
    }

    // ~READJUST/UPDATE CLIENT INFORMATION
    public static async Task<string> UpdateClientAsync (Client client)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = @"UPDATE clients 
                            SET nom = @nom, prenom = @prenom, adresse = @adresse, mail = @mail, contact = @contact
                            WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("nom", client.Nom);
        preparedQuery.Parameters.AddWithValue("prenom", (object?)client.Prenom ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("adresse", client.Adresse);
        preparedQuery.Parameters.AddWithValue("mail", (object?)client.Mail ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("contact", (object?)client.Contact ?? DBNull.Value);
        preparedQuery.Parameters.AddWithValue("idClient", client.IdClient);

        try 
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "success";
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return "error";
        }
    }

    // ~CHANGE PIN!!
    public static async Task<string> ChangePinAsync (string newPin, string idClient)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "UPDATE clients SET pin = @pin WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("pin", newPin);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try 
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "success";
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return "error";
        }
    }

    // ~LOCK/UNLOCK A CLIENT ACCOUNT
    public static async Task<string> LockClientAsync (bool bloque, string idClient, string idEmploye, string codeAgence)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = @"UPDATE clients 
                            SET bloque = @bloque, id_employe = @idEmploye, code_agence = @codeAgence 
                            WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("bloque", bloque);
        preparedQuery.Parameters.AddWithValue("idEmploye", idEmploye);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try 
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "success";
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return "error";
        }
    }

    // ~ USE TO DO A TRANSACTION
    public static async Task<string> DepositAsync (decimal amount, string idClient, NpgsqlConnection conn, NpgsqlTransaction? transaction)
    {
        if (amount <= 0) return "amount should be positif";

        const string query = "UPDATE clients SET solde = solde + @amount WHERE id_client = @idClient FOR UPDATE;";
        using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
        preparedQuery.Parameters.AddWithValue("amount", amount);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                return "no id_client found";
            }
            return $"success";
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!
            return "error";
        }
    }
    public static async Task<string> WithdrawAsync (decimal amount, string idClient, NpgsqlConnection conn, NpgsqlTransaction? transaction)
    {
        if (amount <= 0) return "amount should be positif";

        const string query = "UPDATE clients SET solde = solde - @amount WHERE id_client = @idClient AND solde >= @amount FOR UPDATE;";
        using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
        preparedQuery.Parameters.AddWithValue("amount", amount);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            int rowAffected = await preparedQuery.ExecuteNonQueryAsync();
            if (rowAffected == 0)
            {
                return "no id_client found";
            }
            return $"success";
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!;
            return "error";
        }
    }
    
    // ~CONSULTER SOLDE
    public static async Task<decimal> ConsulterSoldeAsync (string idClient)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "SELECT solde FROM clients WHERE id_client = @idClient;";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            object? solde = await preparedQuery.ExecuteScalarAsync();
            if (solde == null || solde == DBNull.Value)
            {
                throw new InvalidOperationException("Client non existant");
            }
            return (solde as decimal?) ?? 0.00m;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!
            throw;
        }
    }

    // ~ GET LIST OF CLIENT
    public static async Task<List<Client>> GetClientListAsync (string? idClient = null, string? idEmploye = null, bool? bloque = null, string? nom = null)
    {
        var listClient = new List<Client>();

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "SELECT * FROM clients WHERE (id_client = @idClient OR @idClient IS NULL) AND (bloque = @bloque OR @bloque IS NULL) AND (id_employe = @idEmploye OR @idEmploye IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL));";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("bloque", bloque == null ? DBNull.Value : bloque);
        preparedQuery.Parameters.AddWithValue("idClient", idClient == null ? DBNull.Value : idClient);
        preparedQuery.Parameters.AddWithValue("idEmpoye", idEmploye == null ? DBNull.Value : idEmploye);
        preparedQuery.Parameters.AddWithValue("nom", nom == null ? DBNull.Value : nom);
        try 
        {
            using var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    IdClient = row.GetString(0),
                    Nom = row.GetString(1),
                    Prenom = row.IsDBNull(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Mail = row.IsDBNull(4) ? null : row.GetString(4),
                    Contact = row.IsDBNull(5) ? null : row.GetString(5),
                    Solde = row.GetDecimal(6),
                    DateCreation = row.GetDateTime(8),
                    Bloque = row.GetBoolean(9),
                    Dette = row.GetDecimal(10),
                    IdEmploye = row.IsDBNull(11) ? null : row.GetString(11),
                    CodeAgence = row.IsDBNull(12) ? null : row.GetString(12)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!
            throw;
        }
    }

}