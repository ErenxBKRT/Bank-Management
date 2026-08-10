using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public record Argent (decimal Solde, decimal Credit);

public static class ServiceCompte
{
    public static async Task<Result> CreateAsync (int refClient, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            Result verify = await ServiceClient.VerifyAsync(refClient, kaeru, kaeruTransac);
            if (!verify.Status)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, verify.Message);
            }

            using NpgsqlCommand isClientLocked = new ("SELECT * FROM client WHERE bloque = true;", kaeru, kaeruTransac);
            if (await isClientLocked.ExecuteScalarAsync() == null)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, verify.Message);
            }
            
            DateTime now = DateTime.Now;
            string microSecond = now.ToString("fffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100000);
            string numero = microSecond + randomNumber.ToString("D5");

            using NpgsqlCommand preparedQuery = new ("INSERT INTO compte (numero, pin, refclient) VALUES (@numero, @pin, @refClient) FOR UPDATE;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Carte créée avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return new (false, "Creation de la carte a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return new (false, "Creation de la carte a échoué.");
        }
    }

    public static async Task<Result> LogInAsync (string numero, string pin, NpgsqlConnection? kaeru = null, NpgsqlTransaction? kaeruTransac = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal) kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        
        try
        {
            using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero AND pin = @pin;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin);

            if (await preparedQuery.ExecuteScalarAsync() == null)
            {
                return new (false, "Pin ou Numero incorrect");
            }
            return new (true, "Connection réussie");
        }
        catch (NpgsqlException ex) 
        {
            Debug.WriteLine(ex.Message);
            return new (false, "Connection échoué");
        }
        catch (Exception ex) 
        {
            Debug.WriteLine(ex.Message);
            return new (false, "Connection échoué");
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    public static async Task<Result> ChangePinAsync (string numero, string pin, string newPin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            Result isLogged = await LogInAsync(numero, pin);
            if (!isLogged.Status)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, isLogged.Message);
            }

            Result isCardLocked = await IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCardLocked.Status)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, isCardLocked.Message);
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE compte SET pin = @pin WHERE numero = @numero FOR UPDATE", kaeru);
            preparedQuery.Parameters.AddWithValue("pin", newPin);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Le pin a été modifié avec succès.");
        } 
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return new (false, "L'opération a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return new (false, "L'opération a échoué.");
        }
    }

    public static async Task<Result> LockAsync (bool bloquer, int? refClient = null, string? numero = null, NpgsqlConnection? kaeru = null, NpgsqlTransaction? kaeruTransac = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal) kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("UPDATE compte SET bloquer = @bloquer WHERE (numero = @numero OR @numero IS NULL) AND (refclient = @refClient OR @refClient IS NULL);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("bloquer", bloquer);
            preparedQuery.Parameters.AddWithValue("refClient", refClient ?? (object)DBNull.Value);

            if (await preparedQuery.ExecuteNonQueryAsync() == 0) return new (false, "Le numero de compte est incorrect");
            if (bloquer) return new (true, "Le client a été bloqué avec succès.");
            return new (true, "Le client a été débloqué avec succès.");
        } 
        catch (NpgsqlException ex) 
        {
            Debug.WriteLine(ex.Message);
            return new (false, "Opération échoué.");
        }
        catch (Exception ex) 
        {
            Debug.WriteLine(ex.Message);
            return new (false, "Opération échoué.");
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    public static async Task<Result> IsLockedAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null)
    {
            using NpgsqlCommand preparedQuery = new ("SELECT carte_bloquer FROM carte_bancaire WHERE num_compte = @numero;", kaeru, transaction);
            preparedQuery.Parameters.AddWithValue("numero", numero);

            object? status = await preparedQuery.ExecuteScalarAsync();
            bool bloquer = Convert.ToBoolean(status);

            if (status == null) return new (false, "Le numero de compte est incorrect.");
            return new (bloquer, "Le compte est bloqué.");
    }

    public static async Task<Result> VerifyAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        if (await preparedQuery.ExecuteScalarAsync() == null)
        {
            return new (false, "Le numero est incorrect.");
        }
        return new (true, "Vérifié");
    }

}
