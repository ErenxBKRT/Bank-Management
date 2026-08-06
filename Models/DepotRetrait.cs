using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class DepotRetrait
{
    public static async Task DepositAsync (decimal montant, int refClient, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("UPDATE client SET solde = solde + @montant WHERE id_client = @refClient FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("montant", montant);
        preparedQuery.Parameters.AddWithValue("refClient", refClient);

        try
        {
            await preparedQuery.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    public static async Task<string> DepotBancaireAsync (string numero, decimal montant, string codeAgence)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        string verify = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
        if (verify != "VERIFIED")
        {
            await kaeruTransac.RollbackAsync();
            return verify;
        }

        int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
        if (refClient == 0)
        {
            await kaeruTransac.RollbackAsync();
            return "Le numero de compte est incorrect";
        }
        string isClientLocked = await ServiceClient.IsLockedAsync(refClient, kaeru, kaeruTransac);
        if (isClientLocked != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return isClientLocked;
        }
        string isCardLocked = await ServiceCarte.IsLockedAsync(numero, kaeru, kaeruTransac);
        if (isCardLocked != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return isCardLocked;
        }
        if (montant <= 0) return "Le montant doit être positif";
        await DepositAsync(montant, refClient, kaeru, kaeruTransac);

        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");
        
        try
        {
            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, refclient, code_agence, status) VALUES (@code, 'Depot', @montant, @refClient, @codeAgence, 'EN ATTENTE');", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Depot en cours...";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            throw new NpgsqlException(ex.Message);
        }
    }

    public static async Task WithdrawAsync (decimal montant, int refClient, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac, string? pin = null)
    {
        using NpgsqlCommand preparedQuery = new ("UPDATE client SET solde = solde - @montant WHERE id_client = @refClient AND solde >= @montant AND (pin = @pin OR pin IS NULL) FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("montant", montant);
        preparedQuery.Parameters.AddWithValue("refClient", refClient);
        preparedQuery.Parameters.AddWithValue("pin", pin ?? (object)DBNull.Value);

        try
        {
            await preparedQuery.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    public static async Task<string> RetraitBancaireAsync (string numero, string pin, decimal montant, string codeAgence)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        string verify = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
        if (verify != "VERIFIED")
        {
            await kaeruTransac.RollbackAsync();
            return verify;
        }

        int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
        if (refClient == 0)
        {
            await kaeruTransac.RollbackAsync();
            return "Le numero de compte est incorrect";
        }
        string isClientLocked = await ServiceClient.IsLockedAsync(refClient, kaeru, kaeruTransac);
        if (isClientLocked != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return isClientLocked;
        }
        string isCardLocked = await ServiceCarte.IsLockedAsync(numero, kaeru, kaeruTransac);
        if (isCardLocked != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return isCardLocked;
        }
        if (montant <= 0) return "Le montant doit être positif";
        await WithdrawAsync(montant, refClient, kaeru, kaeruTransac, pin);

        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        try
        {
            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, refclient, code_agence, status) VALUES (@code, 'Retrait', @montant, @refClient, @codeAgence, 'EN ATTENTE');", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "Retrait en cours...";
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            throw new NpgsqlException(ex.Message);
        }
    }

}
