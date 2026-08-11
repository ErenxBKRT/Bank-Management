using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public record Argent (decimal Solde, decimal Credit);

public static class ServiceCompte
{
    // creation d'un nouveau compte bancaire
    public static async Task<Result> CreateAsync (int refClient, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            // verifie si l'id du client est correct
            using NpgsqlCommand verifyClient = new ("SELECT * FROM client WHERE id_client = @idClient FOR UPDATE;", kaeru, kaeruTransac);
            verifyClient.Parameters.AddWithValue("idClient", refClient);
            if (await verifyClient.ExecuteScalarAsync() == null)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "L'identifiant du client est incorrect.");
            }

            // verifie si le client n'est pas bloqué
            using NpgsqlCommand isClientLocked = new ("SELECT * FROM client WHERE bloquer = true FOR UPDATE;", kaeru, kaeruTransac);
            if (await isClientLocked.ExecuteScalarAsync() != null)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "Le client est bloqué");
            }
            
            DateTime now = DateTime.Now;
            string microSecond = now.ToString("fffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100000);
            string numero = microSecond + randomNumber.ToString("D5");

            using NpgsqlCommand preparedQuery = new ("INSERT INTO compte (numero, pin, refclient) VALUES (@numero, @pin, @refClient);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Compte créée avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Creation du compte a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Creation du compte a échoué.");
        }
    }

    public static async Task<LoginAccountClient> LogInAsync (string numero, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        
        try
        {
            using NpgsqlCommand isLocked = new ("SELECT * FROM compte WHERE bloquer = true AND numero = @numero;", kaeru);
            isLocked.Parameters.AddWithValue("numero", numero);
            if (await isLocked.ExecuteScalarAsync() != null)
            {
                return new (false, "Le compte est bloqué");
            }
            
            using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero AND pin = @pin;", kaeru);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("pin", pin);
            
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            if (!row.HasRows)
            {
                return new (false, "Identifiant ou Pin incorrect");
            }
            await row.ReadAsync();
            Compte compte = new()
            {
                Numero = row.GetString(0),
                Solde = row.GetDecimal(1),
                Credit = row.GetDecimal(2),
                Bloque = row.GetBoolean(3)
            };
            return new (true, "Connection réussie", compte);
        }
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message);
            throw;
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    //changer le pin du compte
    public static async Task<Result> ChangePinAsync (string numero, string newPin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            // normalement inutile de verifier si le compte est bloqué, il ne peut se connecter si son compte est bloqué
            Result isCompteLocked = await IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCompteLocked.Status)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, isCompteLocked.Message);
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE compte SET pin = @pin WHERE numero = @numero", kaeru);
            preparedQuery.Parameters.AddWithValue("pin", newPin);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Le pin a été modifié avec succès.");
        } 
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "L'opération a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "L'opération a échoué.");
        }
    }

    // bloqué un compte sans bloqué le client
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
            if (bloquer) return new (true, "Le compte a été bloqué avec succès.");
            return new (true, "Le compte a été débloqué avec succès.");
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine(ex.Message);
            return new (false, "Opération échoué.");
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.Message);
            return new (false, "Opération échoué.");
        }
        finally { if (disposeAtFinal && kaeru != null) await kaeru.DisposeAsync(); }
    }

    // verifie si le compte est bloqué
    public static async Task<Result> IsLockedAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? transaction = null)
    {
            using NpgsqlCommand preparedQuery = new ("SELECT bloquer FROM compte WHERE numero = @numero FOR UPDATE;", kaeru, transaction);
            preparedQuery.Parameters.AddWithValue("numero", numero);

            object? status = await preparedQuery.ExecuteScalarAsync();
            bool bloquer = Convert.ToBoolean(status);

            if (status == null) return new (false, "Le numero de compte est incorrect.");
            return new (bloquer, "Le compte est bloqué.");
    }

    // verifie si le numero est exact
    public static async Task<Result> VerifyAsync (string numero, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM compte WHERE numero = @numero FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("numero", numero);

        if (await preparedQuery.ExecuteScalarAsync() == null)
        {
            return new (false, "Le numero est incorrect.");
        }
        return new (true, "Vérifié");
    }

}
