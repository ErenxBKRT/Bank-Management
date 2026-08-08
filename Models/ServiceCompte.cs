using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceCompte
{
    public static async Task<string> CreateAsync (int refClient, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verify = await ServiceClient.VerifyAsync(refClient, kaeru, kaeruTransac);
            if (verify != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verify;
            }

            using NpgsqlCommand isClientLocked = new ("SELECT * FROM client WHERE bloque = true;", kaeru, kaeruTransac);
            if (await isClientLocked.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return "Le client est bloqué.";
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
            return "Carte créée avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Creation de la carte a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Creation de la carte a échoué.";
        }
    }

    public static async Task<bool> LogInAsync (string numero, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero AND pin = @pin;", kaeru);
        preparedQuery.Parameters.AddWithValue("numero", numero);
        preparedQuery.Parameters.AddWithValue("pin", pin);

        object? logged = await preparedQuery.ExecuteScalarAsync();
        if (logged == null)
        {
            return false;
        }
        return true;
    }

    public static async Task<string> ChangePinAsync (string numero, string pin, string newPin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            using NpgsqlCommand isCorrect = new ("SELECT * FROM compte WHERE numero = @numero AND pin = @pin;", kaeru, kaeruTransac);
            isCorrect.Parameters.AddWithValue("numero", numero);
            isCorrect.Parameters.AddWithValue("pin", pin);
            
            if (await isCorrect.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return "Pin ou identifiant incorrect.";
            }

            string isCardLocked = await IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCardLocked != "NO")
            {
                await kaeruTransac.RollbackAsync();
                return isCardLocked;
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE compte SET pin = @pin WHERE numero = @numero FOR UPDATE", kaeru);
            preparedQuery.Parameters.AddWithValue("pin", newPin);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Le pin a été modifié avec succès.";
        } 
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "L'opération a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "L'opération a échoué.";
        }
    }

    public static async Task<string> LockAsync (bool bloquer, int? refClient = null, string? numero = null, NpgsqlConnection? kaeru = null, NpgsqlTransaction? kaeruTransac = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal) kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("UPDATE compte SET bloquer = @bloquer WHERE (numero = @numero OR @numero IS NULL) AND (refclient = @refClient OR @refClient IS NULL);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("bloquer", bloquer);
            preparedQuery.Parameters.AddWithValue("refClient", refClient ?? (object)DBNull.Value);

            if (await preparedQuery.ExecuteNonQueryAsync() == 0) return "Le numero de compte est incorrect";
            if (bloquer) return "Le client a été bloqué ave succès.";
            return "Le client a été débloqué avec succès.";
        } 
        catch (NpgsqlException ex) 
        {
            Debug.WriteLine(ex.Message);
            return "Opération échoué.";
        }
        catch (Exception ex) 
        {
            Debug.WriteLine(ex.Message);
            return "Opération échoué.";
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    public static async Task<decimal?> ConsulterSoldeAsync (string numero)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("SELECT solde FROM compte WHERE numero = @numero;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero);

            object? answer = await preparedQuery.ExecuteScalarAsync();
            decimal solde = Convert.ToDecimal(answer);
            await kaeruTransac.CommitAsync();
            return solde;
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return null;
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return null;
        }
    }

    public static async Task<string> IsLockedAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null)
    {
            using NpgsqlCommand preparedQuery = new ("SELECT carte_bloquer FROM carte_bancaire WHERE num_compte = @numero;", kaeru, transaction);
            preparedQuery.Parameters.AddWithValue("numero", numero);

            object? status = await preparedQuery.ExecuteScalarAsync();
            bool bloquer = Convert.ToBoolean(status);

            if (status == null) return "Le numero de compte est incorrect.";
            else if (bloquer == true) return "Le compte est bloqué.";
            return "NO";
    }

    public static async Task<string> VerifyAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        if (await preparedQuery.ExecuteNonQueryAsync() == 0)
        {
            return "Le numero est incorrect.";
        }
        return "VERIFIED";
    }

}
