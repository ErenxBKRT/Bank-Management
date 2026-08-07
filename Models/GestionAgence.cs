using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public static class GestionAgence
{
    public static async Task<string> AddAsync (string adresse, decimal solde)
    {
        DateTime now = DateTime.Now;
        string code = now.ToString("ffff");
        
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("INSERT INTO agence (code_agence, adresse_agence, solde) VALUES (@code, @adresse, @solde);", kaeru);
        preparedQuery.Parameters.AddWithValue("code", code);
        preparedQuery.Parameters.AddWithValue("adresse", adresse);
        preparedQuery.Parameters.AddWithValue("solde", solde);

        try
        {
            await preparedQuery.ExecuteNonQueryAsync();
            return "Agence ajouté avec succes.";
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine($"Error : {ex.Message}");
            return "L'ajout du nouvel agence a échoué.";
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error : {ex.Message}");
            return "L'ajout du nouvel agence a échoué.";
        }
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
            Debug.WriteLine($"Error : {ex.Message}");
            return "Mis à jour des information de l'agence a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "Mis à jour des information de l'agence a échoué.";
        }
    }

    public static async Task<string> VerifyCodeAsync (string codeAgence, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

        if (await preparedQuery.ExecuteNonQueryAsync() == 0)
        {
            return "Le code agence n'existe pas";
        }
        return "VERIFIED";
    }

    public static async Task<string> DepositAsync (string code, decimal montant)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        if (montant <= 0)
        {
            await kaeruTransac.RollbackAsync();
            return "Le montant doit être positif.";
        }

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
            Debug.WriteLine($"Error : {ex.Message}");
            return "Le dépôt a echoué.";
        }
    }

}
