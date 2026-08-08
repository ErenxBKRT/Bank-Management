using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class CreditVirement
{
    public static async Task<string> CreditAsync (string numero, string codeAgence, decimal montant)
    {
        if (montant <= 0) return "Le montant doit être positif.";

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verifyCompte = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (verifyCompte != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyCompte;
            }
            
            string verifyAgence = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (verifyAgence != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyAgence;
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            NpgsqlCommand compareSolde = new ("SELECT solde FROM agence WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            decimal solde = (decimal?)await compareSolde.ExecuteScalarAsync() ?? 0.00m;
            if (solde < montant)
            {
                await kaeruTransac.RollbackAsync();
                return "Le solde de l'agence est insufisant.";
            }

            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant WHERE numero = @numero FOR UPDATE;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();

            using NpgsqlCommand getCreditFromAgence = new ("UPDATE agence SET solde = solde - @montant WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            await getCreditFromAgence.ExecuteNonQueryAsync();

            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Credit', @montant, @numero, @codeAgence);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            await preparedQuery.ExecuteNonQueryAsync();

            await kaeruTransac.CommitAsync();
            return "Credit envoyé avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Le credit a echoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Le credit a echoué.";
        }
    }

    public static async Task<string> VirementBancaireAsync (decimal montant, string numero,string codeAgence, string nom, string? description = null)
    {
        if (montant <= 0) return "Le montant doit être positif.";
        
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            string verifyCompte = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (verifyCompte != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyCompte;
            }
            
            string isCardLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCardLocked != "NO")
            {
                await kaeruTransac.RollbackAsync();
                return isCardLocked;
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant WHERE numero = @numero FOR UPDATE;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();

            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, nom, numero, code_agence, description) VALUES (@code, 'Virement', @montant, @nom, @numero, @codeAgence, @description);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            preparedQuery.Parameters.AddWithValue("description", description ?? (object)DBNull.Value);
            
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Virement bancaire effectué avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Le virement a echoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine(ex.Message);
            return "Le virement a echoué.";
        }
    }

}
