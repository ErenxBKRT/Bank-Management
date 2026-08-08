using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class DepotRetrait
{
    public static async Task<string> DepositAsync (string numero, decimal montant, string codeAgence)
    {
        if (montant <= 0) return "Le montant doit être positif";

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verifyAccount = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (verifyAccount != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyAccount;
            }
            string isAccountLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isAccountLocked != "NO")
            {
                await kaeruTransac.RollbackAsync();
                return isAccountLocked;
            }

            string verifyCode = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (verifyCode != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyCode;
            }
        
            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant WHERE numero = @numero FOR UPDATE;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");
            
            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Depot', @montant, @numero, @codeAgence) FOR UPDATE;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Dépot réussie";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "Le dépôt a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "Le dépôt a échoué.";
        }
    }

    public static async Task<string> WithdrawAsync (string numero, string pin, decimal montant, string codeAgence)
    {
        if (montant <= 0) return "Le montant doit être positif";

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            string verifyAccount = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (verifyAccount != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyAccount;
            }

            string isCardLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCardLocked != "NO")
            {
                await kaeruTransac.RollbackAsync();
                return isCardLocked;
            }

            string verifyAgence = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (verifyAgence != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verifyAgence;
            }

            using NpgsqlCommand withdraw = new ("UPDATE compte SET solde = solde - @montant WHERE numero = @numero AND solde >= @montant AND pin = @pin FOR UPDATE;", kaeru, kaeruTransac);
            withdraw.Parameters.AddWithValue("montant", montant);
            withdraw.Parameters.AddWithValue("numero", numero);
            withdraw.Parameters.AddWithValue("pin", pin);
            if (await withdraw.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return "Solde insuffisant.";
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Retrait', @montant, @numero, @codeAgence);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Retrait terminé avec succès.";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "Le retrait a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "Le retrait a échoué.";
        }
    }

}
