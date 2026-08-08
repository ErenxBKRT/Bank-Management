using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public static class GestionAgence
{
    public static async Task<string> AddAsync (string adresse, decimal solde, string pin)
    {
        DateTime now = DateTime.Now;
        string code = now.ToString("ffff");
        
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("INSERT INTO agence (code_agence, adresse_agence, solde, pin) VALUES (@code, @adresse, @solde, @pin);", kaeru);
        preparedQuery.Parameters.AddWithValue("code", code);
        preparedQuery.Parameters.AddWithValue("adresse", adresse);
        preparedQuery.Parameters.AddWithValue("solde", solde);
        preparedQuery.Parameters.AddWithValue("pin", pin);

        try
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "Agence ajouté avec succes.";
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            return "L'ajout du nouvel agence a échoué.";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            return "L'ajout du nouvel agence a échoué.";
        }
    }

    public static async Task<bool> LogInAsync (string code, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @code AND pin = @pin;", kaeru);
        preparedQuery.Parameters.AddWithValue("code", code);
        preparedQuery.Parameters.AddWithValue("pin", pin);

        object? logged = await preparedQuery.ExecuteScalarAsync();
        if (logged == null)
        {
            return false;
        }
        return true;
    }

    public static async Task<string> UpdateAsync (string code, string adresse)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verify = await VerifyCodeAsync(code, kaeru, kaeruTransac);
            if (verify != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verify;
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE agence SET adresse = @adresse WHERE code_agence = @code;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("adresse", adresse);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Mis à jour des informations terminé avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return "Mis à jour des information de l'agence a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return "Mis à jour des information de l'agence a échoué.";
        }
    }

    public static async Task<string> VerifyCodeAsync (string codeAgence, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

        if (await preparedQuery.ExecuteNonQueryAsync() == 0)
        {
            return "Le code agence n'existe pas.";
        }
        return "VERIFIED";
    }

    public static async Task<string> DepositAsync (string code, decimal montant)
    {
        if (montant <= 0)
        {
            return "Le montant doit être positif.";
        }

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verify = await VerifyCodeAsync (code, kaeru, kaeruTransac);
            if (verify != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verify;
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE agence SET solde = solde + @montant WHERE code_agence = @code;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Dépôt effectué avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return "Le dépôt a echoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return "Le dépôt a echoué.";
        }
    }

}
