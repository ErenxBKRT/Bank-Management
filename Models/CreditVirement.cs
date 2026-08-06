using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class CreditVirement
{
    public static async Task<string> DemanderCreditAsync (string numero, string codeAgence, decimal montant)
    {
        if (montant <= 0) return "Le montant doit être positif";

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
        if (refClient == 0)
        {
            return "Le numero du compte est incorrect";
        }

        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        try
        {            
            NpgsqlCommand compareSolde = new ("SELECT solde FROM agence WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            decimal? solde = (decimal?)await compareSolde.ExecuteScalarAsync();
            if (solde == null)
            {
                await kaeruTransac.RollbackAsync();
                return "Le code agence est incorrect";
            }
            else if (solde < montant)
            {
                await kaeruTransac.RollbackAsync();
                return "Le solde de l'agence est insufisant";
            }

            NpgsqlCommand updateClientCredit = new ("UPDATE client SET credit = credit + @montant WHERE id_client = @refClient; FOR UPDATE", kaeru, kaeruTransac);
            updateClientCredit.Parameters.AddWithValue("refClient", refClient);

            NpgsqlCommand getCreditFromAgence = new ("UPDATE agence SET solde = solde - @montant WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            await getCreditFromAgence.ExecuteNonQueryAsync();

            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence, status) VALUES (@code, 'Credit', @montant, @num_compte, @codeAgence, 'EN ATTENTE');", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("num_compte", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            await preparedQuery.ExecuteNonQueryAsync();

            await kaeruTransac.CommitAsync();
            return "Credit en cours de traitement...";
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    public static async Task<string> VirementBancaireAsync (decimal montant, string numero,string codeAgence, string nom)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
        if (refClient == 0) 
        {
            await kaeruTransac.RollbackAsync();
            return "Le numero de compte est incorrect";
        }
        string statusClient = await ServiceClient.IsLockedAsync(refClient, kaeru, kaeruTransac);
        if (statusClient != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return statusClient;
        }
        string statusCard = await ServiceCarte.IsLockedAsync(numero, kaeru, kaeruTransac);
        if (statusCard != "NO")
        {
            await kaeruTransac.RollbackAsync();
            return statusCard;
        }

        if (montant <= 0) return "Le montant doit être positif";

        await DepotRetrait.DepositAsync(montant, refClient, kaeru, kaeruTransac);
        
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        try 
        {
            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, nom, refclient, code_agence, status) VALUES (@code, 'Virement', @montant, @nom, @refClient, @codeAgence, 'EN ATTENTE');", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return "succes";
        }
        catch (NpgsqlException ex)
        {
            throw new NpgsqlException(ex.Message);
        }
    }

    // ~ DEPOT BANCAIRE
}
