using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceCarte
{
    // CREER CARTE BANCAIRE 
    public static async Task<string> CreateAsync (int refClient, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        string? status = await ServiceClient.IsLockedAsync(refClient, kaeru, kaeruTransac);
        if (status != "SUCCESS")
        {
            await kaeruTransac.RollbackAsync();
            return status;
        }
        
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("fffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100000);
        string numero = microSecond + randomNumber.ToString("D5");

        try
        {

            using NpgsqlCommand preparedQuery = new ("INSERT INTO carte_bancaire (num_compte, pin, refclient) VALUES (@numero, @pin, @refClient);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "SUCCESS";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            throw new NpgsqlException (ex.Message);
        }
    }

    // ~CHANGE PIN!!
    public static async Task<string> ChangePinAsync (string numero, string newPin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("UPDATE carte_bancaire SET pin = @pin WHERE num_compte = @numero", kaeru);
        preparedQuery.Parameters.AddWithValue("pin", newPin);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        try 
        {

            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                return "Le numero de compte est incorrect";
            }
            return "SUCCESS";
        } 
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    // GET CLIENT ID FROM CARD NUMBER AND PIN
    public static async Task<int> GetIdAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null, string? pin = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT refclient FROM carte_bancaire WHERE num_compte = @numero AND (pin = @pin OR @pin IS NULL);", kaeru, transaction);
        preparedQuery.Parameters.AddWithValue("numero", numero);
        preparedQuery.Parameters.AddWithValue("pin", pin ?? (object)DBNull.Value);
        
        try 
        {
            object? id = await preparedQuery.ExecuteScalarAsync();
            int refClient = Convert.ToInt32(id);
            if (refClient == 0) return 0;
            return refClient;
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    // CARTE BLOQUER ET DEBLOQUER
    public static async Task<string> LockAsync (bool bloquer, int? refClient = null, string? numero = null, NpgsqlConnection? kaeru = null, NpgsqlTransaction? transaction = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal) kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        using NpgsqlCommand preparedQuery = new ("UPDATE carte_bancaire SET carte_bloquer = @bloquer WHERE (num_compte = @numero OR @numero IS NULL) AND (refclient = @refClient OR @refClient IS NULL);", kaeru, transaction);
        preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("bloquer", bloquer);
        preparedQuery.Parameters.AddWithValue("refClient", refClient ?? (object)DBNull.Value);

        try
        {
            if (await preparedQuery.ExecuteNonQueryAsync() == 0) return "Le numero de compte est incorrect";
            return "SUCCESS";
        } 
        catch (NpgsqlException ex) 
        {
            throw new NpgsqlException(ex.Message);
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    public static async Task<string> IsLockedAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT carte_bloquer FROM carte_bancaire WHERE num_compte = @numero;", kaeru, transaction);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        try
        {
            object? status = await preparedQuery.ExecuteScalarAsync();
            bool bloquer = Convert.ToBoolean(status);

            if (status == null) return "Le numero de compte est incorrect";
            else if (bloquer == true) return "Le compte est bloqué";
            return "NO";
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

}
