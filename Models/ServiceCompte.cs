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
                Numero = row.GetString(row.GetOrdinal("numero")),
                Solde = row.GetDecimal(row.GetOrdinal("solde")),
                Credit = row.GetDecimal(row.GetOrdinal("credit")),
                Bloque = row.GetBoolean(row.GetOrdinal("bloquer"))
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
    // Changer le pin du compte (Version corrigée)
    public static async Task<Result> ChangePinAsync(string numero, string oldPin, string newPin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            // 1. Vérifier si le compte existe et si l'ancien PIN est correct
            using NpgsqlCommand verifyPinQuery = new("SELECT * FROM compte WHERE numero = @numero AND pin = @pin FOR UPDATE;", kaeru, kaeruTransac);
            verifyPinQuery.Parameters.AddWithValue("numero", numero);
            verifyPinQuery.Parameters.AddWithValue("pin", oldPin);

            if (await verifyPinQuery.ExecuteScalarAsync() == null)
            {
                await kaeruTransac.RollbackAsync();
                return new(false, "L'ancien PIN est incorrect.");
            }

            // 2. Vérifier si le compte est bloqué
            Result isCompteLocked = await IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCompteLocked.Status) // Si Status == true, alors le compte est bloqué
            {
                await kaeruTransac.RollbackAsync();
                return new(false, "Impossible de modifier le PIN : le compte est bloqué.");
            }

            // 3. Mise à jour du PIN avec la transaction associée
            using NpgsqlCommand preparedQuery = new("UPDATE compte SET pin = @pin WHERE numero = @numero;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("pin", newPin);
            preparedQuery.Parameters.AddWithValue("numero", numero);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();

            return new(true, "Le code PIN a été modifié avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new(false, "L'opération a échoué due à une erreur réseau/BDD.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new(false, "L'opération a échoué.");
        }
    }
    // bloqué un compte sans bloqué le client
    public static async Task<Result> LockAsync (bool bloquer, int? refClient = null, string? numero = null, NpgsqlConnection? kaeru = null, NpgsqlTransaction? kaeruTransac = null)
    {
        bool disposeAtFinal = kaeru == null;
        if (disposeAtFinal) kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand isLocked = new ("SELECT * FROM client JOIN compte ON compte.refclient = client.id_client WHERE client.bloquer = true AND (compte.numero = @numero OR @numero IS NULL);", kaeru);
            isLocked.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            if (await isLocked.ExecuteScalarAsync() != null)
            {
                Console.WriteLine("already Locked");
                return new (false, "Le client est deja bloque");
            }

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
