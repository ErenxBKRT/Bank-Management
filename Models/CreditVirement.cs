using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class CreditVirement
{
    /*
    utiliser pour faire un demande de credit, les parametre ne sont pas optionelle
    */
    public static async Task<Result> CreditAsync (string numero, string codeAgence, decimal montant)
    {
        if (montant <= 0) 
        {
            return new (false, "Le montant doit être positif.");
        }

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            //verifie si le compte existe, le numero est correct
            Result verifyCompte = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (!verifyCompte.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyCompte;
            }
            
            //verifie si le code de l'agence est correct
            Result verifyAgence = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (!verifyAgence.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyAgence;
            }

            // verifie si le client a un credit qui n'est pas encore payé
            using NpgsqlCommand canDoCredit = new ("SELECT * FROM compte WHERE numero = @numero AND credit > 0 FOR UPDATE;", kaeru, kaeruTransac);
            canDoCredit.Parameters.AddWithValue("numero", numero);
            if (await canDoCredit.ExecuteScalarAsync() != null)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "Veuillez régler les ancienne credit non payé");
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            // verifie si le solde de l'agence n'est pas suffisant pour envoyé le credit
            NpgsqlCommand compareSolde = new ("SELECT solde FROM agence WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            decimal solde = (decimal?)await compareSolde.ExecuteScalarAsync() ?? 0.00m;
            if (solde < montant)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "solde agence insuffisant");
            }

            using NpgsqlCommand getCreditFromAgence = new ("UPDATE agence SET solde = solde - @montant WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            await getCreditFromAgence.ExecuteNonQueryAsync();

            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant, credit = credit + @montant WHERE numero = @numero;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();


            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Credit', @montant, @numero, @codeAgence);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            await preparedQuery.ExecuteNonQueryAsync();

            await kaeruTransac.CommitAsync();
            return new (true, "Credit envoyé avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le credit a echoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le credit a echoué.");
        }
    }

    /*
    payé le credit que le client a emprunté
    */
    public static async Task<Result> PayerCreditAsync (string numero, string codeAgence, decimal montant)
    {
        if (montant <= 0) 
        {
            return new (false, "Le montant doit être positif.");
        }

        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try
        {
            //verifie si le compte existe, le numero est correct
            Result verifyCompte = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (!verifyCompte.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyCompte;
            }
            
            //verifie si le code de l'agence est correct
            Result verifyAgence = await GestionAgence.VerifyCodeAsync(codeAgence, kaeru, kaeruTransac);
            if (!verifyAgence.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyAgence;
            }

            // verifie que le client a un credit a payé
            using NpgsqlCommand canDoCredit = new ("SELECT * FROM compte WHERE numero = @numero AND credit = 0 FOR UPDATE;", kaeru, kaeruTransac);
            canDoCredit.Parameters.AddWithValue("numero", numero);
            if (await canDoCredit.ExecuteScalarAsync() != null)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "Vous n'avez aucun credit non payé");
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            using NpgsqlCommand getCreditFromAgence = new ("UPDATE agence SET solde = solde + @montant WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            await getCreditFromAgence.ExecuteNonQueryAsync();

            using NpgsqlCommand deposit = new ("UPDATE compte SET credit = credit - @montant WHERE numero = @numero;", kaeru, kaeruTransac);
            deposit.Parameters.AddWithValue("montant", montant);
            deposit.Parameters.AddWithValue("numero", numero);
            await deposit.ExecuteNonQueryAsync();


            using NpgsqlCommand preparedQuery = new ("INSERT INTO transaction (code, libelle, montant, numero, code_agence) VALUES (@code, 'Credit', @montant, @numero, @codeAgence);", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("numero", numero);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            await preparedQuery.ExecuteNonQueryAsync();

            await kaeruTransac.CommitAsync();
            return new (true, "Credit payé avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le payment a echoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le payment a echoué.");
        }
    }

    /*
    envoyé un virement vers un compte et utilise le nom que l'expediteur utilise.
    */
    public static async Task<Result> VirementAsync (string numero, string codeAgence, decimal montant, string nom, string description)
    {
        if (montant <= 0) return new (false, "Le montant doit être positif.");
        
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            //verifie si le compte existe, le numero est correct
            Result verifyCompte = await ServiceCompte.VerifyAsync(numero, kaeru, kaeruTransac);
            if (!verifyCompte.Status)
            {
                await kaeruTransac.RollbackAsync();
                return verifyCompte;
            }
            
            //verifie si le code de l'agence est correct
            Result isCompteLocked = await ServiceCompte.IsLockedAsync(numero, kaeru, kaeruTransac);
            if (isCompteLocked.Status)
            {
                await kaeruTransac.RollbackAsync();
                return isCompteLocked;
            }

            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
            string code = microSecond + "-" + randomNumber.ToString("D2");

            using NpgsqlCommand deposit = new ("UPDATE compte SET solde = solde + @montant WHERE numero = @numero;", kaeru, kaeruTransac);
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
            return new (true, "Virement bancaire effectué avec succès.");
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le virement a echoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return new (false, "Le virement a echoué.");
        }
    }

}
