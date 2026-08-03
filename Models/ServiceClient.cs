using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bankmanaging.Models;
public static class ServiceClient
{
    // ~ INSERT THE NEW CLIENT INTO THE DATABASE 
    public static async Task<bool> CreateClientAsync (string nom, string adresse, string contact, string? prenom)
    {

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "INSERT INTO client (nom, prenom, adresse, contact) VALUES (@nom, @prenom, @adresse, @contact);";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("nom", nom);
        preparedQuery.Parameters.AddWithValue("prenom", prenom ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("adresse", adresse);
        preparedQuery.Parameters.AddWithValue("contact", contact);
        
        try 
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return true;
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }

    // ~READJUST/UPDATE CLIENT INFORMATION
    public static async Task<bool> UpdateClientAsync (int idClient, string nom, string adresse, string contact, string? prenom)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "UPDATE client SET nom = @nom, prenom = @prenom, adresse = @adresse, contact = @contact WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("nom", nom);
        preparedQuery.Parameters.AddWithValue("prenom", prenom ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("adresse", adresse);
        preparedQuery.Parameters.AddWithValue("contact", contact);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try 
        {
            if(await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("aucun ID trouve");
                return false;
            }
            return true;
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }

    // ~CHANGE PIN!!
    public static async Task<bool> ChangePinAsync (string newPin, string idClient)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "UPDATE client SET pin = @pin WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("pin", newPin);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try 
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("ID inexistant");
                return false;
            }
            return true;
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }

    // ~LOCK/UNLOCK A CLIENT ACCOUNT
    public static async Task<bool> LockClientAsync (bool bloque, string idClient)
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "UPDATE client SET bloque = @bloque WHERE id_client = @idClient;";

        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("bloque", bloque);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try 
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("ID inexistant");
                return false;
            }
            return true;
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }

    // ~ USE TO DO A TRANSACTION
    public static async Task<bool> DepositAsync (decimal amount, int idClient, NpgsqlConnection conn, NpgsqlTransaction? transaction)
    {
        if (amount <= 0) 
        {
            Console.WriteLine("montant negatif");
            return false;
        }

        const string query = "UPDATE client SET solde = solde + @amount WHERE id_client = @idClient FOR UPDATE;";
        using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
        preparedQuery.Parameters.AddWithValue("amount", amount);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("ID inexistant");
                return false;
            }
            return true;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }
    public static async Task<bool> WithdrawAsync (decimal amount, int idClient, NpgsqlConnection conn, NpgsqlTransaction? transaction)
    {
        if (amount <= 0) 
        {
            Console.WriteLine("montant negatif");
            return false;
        }

        const string query = "UPDATE client SET solde = solde - @amount WHERE id_client = @idClient AND solde >= @amount FOR UPDATE;";
        using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
        preparedQuery.Parameters.AddWithValue("amount", amount);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("ID inexistant");
                return false;
            }
            return true;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
    }
    
    // ~CONSULTER SOLDE
    public static async Task<decimal> ConsulterSoldeAsync (string idClient, NpgsqlConnection conn)
    {
        const string query = "SELECT solde FROM client WHERE id_client = @idClient;";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            object? solde = await preparedQuery.ExecuteScalarAsync();
            if (solde == null || solde == DBNull.Value)
            {
                throw new InvalidOperationException("ID inexistant");
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
    public static async Task<List<Client>> GetClientListAsync (int? idClient = null, bool? bloque = null, string? nom = null)
    {
        var listClient = new List<Client>();

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "SELECT * FROM client WHERE (id_client = @idClient OR @idClient IS NULL) AND (bloque = @bloque OR @bloque IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL));";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("bloque", bloque == null ? DBNull.Value : bloque);
        preparedQuery.Parameters.AddWithValue("idClient", idClient == null ? DBNull.Value : idClient);
        preparedQuery.Parameters.AddWithValue("nom", nom == null ? DBNull.Value : nom);
        try 
        {
            using var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    IdClient = row.GetInt32(0),
                    Nom = row.GetString(1),
                    Prenom = row.IsDBNull(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Contact = row.GetString(4),
                    Solde = row.GetDecimal(5),
                    Bloque = row.GetBoolean(6),
                    Credit = row.GetDecimal(7)
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