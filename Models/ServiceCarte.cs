using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceCarte
{
    // GET CLIENT ID FROM CARD NUMBER AND PIN
    public static async Task<int> GetIdAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null, string? pin = null)
    {
            using NpgsqlCommand preparedQuery = new ("SELECT refclient FROM carte_bancaire WHERE numero = @numero AND (pin = @pin OR @pin IS NULL);", kaeru, transaction);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin ?? (object)DBNull.Value);
            
            object? id = await preparedQuery.ExecuteScalarAsync();
            int refClient = Convert.ToInt32(id);
            if (refClient == 0)
            {
                Console.WriteLine("numero ou pin incorrect");
                return 0;
            }
            return refClient;
    }

    // ~CHANGE PIN!!
    public static async Task<bool> ChangePinAsync (string newPin, string numero)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("UPDATE carte_bancaire SET pin = @pin WHERE num_compte = @numero", kaeru);
        preparedQuery.Parameters.AddWithValue("pin", newPin);
        preparedQuery.Parameters.AddWithValue("numero", numero);

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

    // CARTE BLOQUER ET DEBLOQUER
    public static async Task<bool> LockCarteAsync (bool bloquer, int? idClient = null, string? numero = null, NpgsqlConnection? kaeru = null, NpgsqlTransaction? transaction = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal)
        {
            kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        }

        using NpgsqlCommand preparedQuery = new ("UPDATE carte_bancaire SET carte_bloquer = @bloquer WHERE (num_compte = @numero OR @numero IS NULL) AND (id_client = @idClient OR @idClient IS NULL);", kaeru, transaction);
        preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("bloquer", bloquer);
        preparedQuery.Parameters.AddWithValue("idClient", idClient ?? (object)DBNull.Value);

        try
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                Console.WriteLine("ID ou numero de carte inexistant");
                return false;
            }
            return true;
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message); // temporary!
            return false;
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    // CREER CARTE BANCAIRE 
    public static async Task<bool> CreerCarteAsync (int idClient, string pin)
    {
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("fffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100000);
        string numero = microSecond + "-" + randomNumber.ToString("D5");

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        bool eligible = await ServiceClient.CheckLockedClientAsync(idClient, kaeru);
        if (!eligible)
        {
            Console.WriteLine("le client est bloquer");
            return false;
        }

        using NpgsqlCommand preparedQuery = new ("INSERT INTO carte_bancaire (num_compte, pin, refclient) VALUES (@numero, @pin, @idClient);", kaeru);
        preparedQuery.Parameters.AddWithValue("numero", numero);
        preparedQuery.Parameters.AddWithValue("pin", pin);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        try
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return true;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    public static async Task<bool> EstBloquerAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT carte_bloquer FROM carte_bancaire WHERE num_compte = @numero;", kaeru, transaction);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        try
        {
            object? status = await preparedQuery.ExecuteScalarAsync();
            bool bloque = Convert.ToBoolean(status);
            return bloque;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
    }

}
