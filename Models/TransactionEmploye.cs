using Npgsql;
using System;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class Transaction
{
    // VIREMENT BANCAIRE
    public static async Task<string> VirementBancaireAsync (string libelle, decimal montant, string recepteur, string idEmploye, string codeAgence, string nom)
    {
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string codeTransaction = microSecond + "-" + randomNumber.ToString("D3");

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
                                                     VALUES (@codeTransaction, @libelle, @montant, @nom, @recepteur, @codeAgence, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("libelle", libelle);
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
        }
    }
}