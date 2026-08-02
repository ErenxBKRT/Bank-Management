using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceEmploye
{
    // VIREMENT BANCAIRE
    public static async Task<string> VirementBancaireAsync (decimal montant, string recepteur, string idEmploye, string codeAgence, string nom)
    {
        if (montant <= 0) return "montont doit etre positif";
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string codeTransaction = microSecond + "-" + randomNumber.ToString("D2");

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        await using var transaction = await conn.BeginTransactionAsync();
        try 
        {
            string depot = await ServiceClient.DepositAsync(montant, recepteur, conn, transaction);
            if (depot == "error" || depot == "no id_client found")
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }
            
            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, nom, recepteur, code_agence, id_employe)
                                                     VALUES (@codeTransaction, 'Virement', @montant, @nom, @recepteur, @codeAgence, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("recepteur", recepteur);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            preparedQuery.Parameters.AddWithValue("idEmploye", idEmploye);
            
            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }
            await transaction.CommitAsync();
            return "success";
        }
        catch (NpgsqlException ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            return "error";
        }
    }

    // ~ DEPOT BANCAIRE
    public static async Task<string> DepotBancaireAsync (string idClient, decimal montant, string codeAgence)
    {
        if (montant <= 0) return "montont doit etre positif";
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string codeTransaction = microSecond + "-" + randomNumber.ToString("D2");

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        await using var transaction = await conn.BeginTransactionAsync();
        try
        {
            string depot = await ServiceClient.DepositAsync(montant, idClient, conn, transaction);
            if (depot == "no id found" || depot == "error")
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }

            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, recepteur, code_agence)
                                                    VALUES (@codeTransaction, 'Depot', @montant, @recepteur, @codeAgence);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("recepteur", idClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }
            await transaction.CommitAsync();
            return "success";
        }
        catch (NpgsqlException ex)
        {
            await transaction.RollbackAsync();
            Console.WriteLine(ex.Message);
            return "error";
        }
    }

    // ~ HISTORIQUE TRANSACTION
    public static async 

}
