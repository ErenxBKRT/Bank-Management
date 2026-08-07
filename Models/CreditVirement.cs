using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class CreditVirement
{
    public static async Task<string> DemanderCreditAsync (string numero, string codeAgence, decimal montant)
    {

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {            
            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
            if (refClient == 0)
            {
                await kaeruTransac.RollbackAsync();
                return "Le numero du compte est incorrect.";
            }

            string verify = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (verify != "VERIFIED")
            {
                await kaeruTransac.RollbackAsync();
                return verify;
            }
            
            string IsClientLocked = await ServiceClient.IsLockedAsync(refClient, kaeru, kaeruTransac);
            if (IsClientLocked != "NO")
            {
                await kaeruTransac.RollbackAsync();
                return "Le compte client est bloqué.";
            }

            if (montant <= 0) 
            {
                await kaeruTransac.RollbackAsync();
                return "Le montant doit être positif.";
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            NpgsqlCommand compareSolde = new ("SELECT solde FROM agence WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            decimal? solde = (decimal?)await compareSolde.ExecuteScalarAsync();
            if (solde < montant)
            {
                await kaeruTransac.RollbackAsync();
                return "Le solde de l'agence est insufisant.";
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

    public static async Task<string> VirementBancaireAsync (decimal montant, string numero,string codeAgence, string nom)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
            if (refClient == 0) 
            {
                await kaeruTransac.RollbackAsync();
                return "Le numero de compte est incorrect.";
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

            if (montant <= 0) 
            {
                await kaeruTransac.RollbackAsync();
                return "Le montant doit être positif.";
            }
            await DepotRetrait.DepositAsync(montant, refClient, kaeru, kaeruTransac);
            
            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, nom, refclient, code_agence, status) VALUES (@code, 'Virement', @montant, @nom, @refClient, @codeAgence, 'EN ATTENTE');", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            
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
