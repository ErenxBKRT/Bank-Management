using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public static class GestionAgence
{
    // ajouter un nouveau agence
    public static async Task<Result> AddAsync (string adresse, decimal solde, string pin)
    {
        DateTime now = DateTime.Now;
        string code = now.ToString("ffff");

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("INSERT INTO agence (code_agence, adresse_agence, solde, pin) VALUES (@code, @adresse, @solde, @pin);", kaeru);
        try
        {
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("adresse", adresse);
            preparedQuery.Parameters.AddWithValue("solde", solde);
            preparedQuery.Parameters.AddWithValue("pin", pin);

            await preparedQuery.ExecuteNonQueryAsync();
            return new (true, "Agence ajouté avec succes.");
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "L'ajout du nouvel agence a échoué.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "L'ajout du nouvel agence a échoué.");
        }
    }

    public static async Task<LoginAccountAgence> LogInAsync (string code, string pin)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @code AND pin = @pin;", kaeru);

        try
        {
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("pin", pin);
            
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
            if (!row.HasRows)
            {
                return new (false, "Identifiant ou Pin incorrect");
            }
            await row.ReadAsync();
            Agence agence = new()
            {
                CodeAgence = row.GetString(0),
                Adresse = row.GetString(1),
                Solde = row.GetDecimal(2)
            };
            return new (true, "Connection avec succès", agence);
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

    //sert a verifier si le code de l'agence est exact, uiliser par les autres services
    public static async Task<Result> VerifyCodeAsync (string codeAgence, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

        if (await preparedQuery.ExecuteScalarAsync() == null)
        {
            return new (false, "Le code agence n'existe pas.");
        }
        return new (true, "Code vérifié");
    }

    // deposé ou plutot ajouter de l'argent dans l'agence
    public static async Task<Result> DepositAsync (string code, decimal montant)
    {
        if (montant <= 0)
        {
            return new (false, "Le montant doit être positif.");
        }

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            // verifie le code de l'agence si elle exist
            Result verify = await VerifyCodeAsync (code, kaeru, kaeruTransac);
            if (!verify.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verify;
            }

            using NpgsqlCommand preparedQuery = new ("UPDATE agence SET solde = solde + @montant WHERE code_agence = @code;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Dépôt effectué avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le dépôt a echoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le dépôt a echoué.");
        }
    }

}
