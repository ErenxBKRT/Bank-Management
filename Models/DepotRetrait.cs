using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class DepotRetrait
{
    public static async Task<Result> DepositAsync (string numero, decimal montant, string codeAgence)
    {
        if (montant <= 0) return new (false, "Le montant doit être positif");

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            Result verifyAccount = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (!verifyAccount.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyAccount;
            }
            Result isAccountLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isAccountLocked.Status)
            {
                await kaeruTransac.RollbackAsync();
                return isAccountLocked;
            }

            Result verifyCode = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (!verifyCode.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyCode;
            }
        
            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant WHERE numero = @numero;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");
            
            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Depot', @montant, @numero, @codeAgence);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return new (true, "Dépot réussie");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le dépôt a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le dépôt a échoué.");
        }
    }

    public static async Task<Result> WithdrawAsync (string numero, string pin, decimal montant, string codeAgence)
    {
        if (montant <= 0) return new (false, "Le montant doit être positif");

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            Result verifyAccount = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (!verifyAccount.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyAccount;
            }

            Result isCompteLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCompteLocked.Status)
            {
                await kaeruTransac.RollbackAsync();
                return isCompteLocked;
            }

            Result verifyAgence = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (!verifyAgence.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyAgence;
            }

            using NpgsqlCommand withdraw = new ("UPDATE compte SET solde = solde - @montant WHERE numero = @numero AND solde >= @montant AND pin = @pin;", kaeru, kaeruTransac);
            withdraw.Parameters.AddWithValue("montant", montant);
            withdraw.Parameters.AddWithValue("numero", numero);
            withdraw.Parameters.AddWithValue("pin", pin);
            if (await withdraw.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "Solde insuffisant.");
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
            return new (true, "Retrait terminé avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le retrait a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "Le retrait a échoué.");
        }
    }

}
