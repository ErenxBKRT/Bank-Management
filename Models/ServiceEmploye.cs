using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceEmploye
{
    // VIREMENT BANCAIRE
    public static async Task<string> VirementBancaireAsync (decimal montant, string refClient, string idEmploye, string codeAgence, string nom)
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
            string depot = await ServiceClient.DepositAsync(montant, refClient, conn, transaction);
            if (depot == "error" || depot == "no id_client found")
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }
            
            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, nom, refclient, code_agence, id_employe)
                                                     VALUES (@codeTransaction, 'Virement', @montant, @nom, @refClient, @codeAgence, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
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
    public static async Task<string> DepotBancaireAsync (string idClient, decimal montant, string codeAgence, string idEmploye)
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

            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, refclient, id_employe)
                                                    VALUES (@codeTransaction, 'Depot', @montant, @refclient, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", idClient);
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

    // ~ RETRAIT BANCAIRE
    public static async Task<string> RetraitBancaireAsync (string refClient, decimal montant, string codeAgence, string idEmploye)
    {
        if (montant <= 0) return "montant doit etre positif";
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
            string retrait = await ServiceClient.WithdrawAsync(montant, refClient, conn, transaction);
            if (retrait == "no id found" || retrait == "error")
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }

            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, refclient, code_agence, id_employe)
                                                    VALUES (@codeTransaction, 'Retrait', @montant, @refclient, @codeAgence, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refclient", refClient);
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

    // ~ HISTORIQUE TRANSACTION
    public static async Task<List<Transactions>> HistoriqueTransactionAsync(string? idClient = null, string? libelle = null, string? codeAgence = null)
    {
        var listTransaction = new List<Transactions>();

        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "SELECT * FROM transactions WHERE (id_client = @idClient OR @idClient IS NULL) AND (libelle = @libelle OR @libelle IS NULL) AND (code_agence = @codeAgence OR @codeAgence is NULL));";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("idClient", idClient ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("libelle", libelle ?? (object)DBNull.Value);
        try
        {
            using var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Transactions transaction = new()
                {
                    CodeTransaction = row.GetString(0),
                    Libelle = row.GetString(1),
                    Montant = row.GetDecimal(2),
                    DateTransaction = row.GetDateTime(3),
                    Nom = await row.IsDBNullAsync(4) ? null : row.GetString(4),
                    IdEmploye = await row.IsDBNullAsync(5) ? null : row.GetString(5),
                    CodeAgence = await row.IsDBNullAsync(6) ? null : row.GetString(6),
                    RefClient = await row.IsDBNullAsync(7) ? null : row.GetString(7),
                };
                listTransaction.Add(transaction);
            }
            return listTransaction;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    // ~ LIST CLIENT AYANT CREDIT
    public static async Task<List<Client>> ClientAvecCreditAsync ()
    {
        IDatabaseConnection kaeru = DatabaseConnection.Instance;
        using var conn = kaeru.Connected();
        await conn.OpenAsync();

        const string query = "SELECT * FROM clients WHERE dette > 0.00;";
        using var preparedQuery = new NpgsqlCommand(query, conn);
        var listClient = new List<Client>();

        try
        {
            var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    IdClient = row.GetString(0),
                    Nom = row.GetString(1),
                    Prenom = await row.IsDBNullAsync(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Mail = await row.IsDBNullAsync(4) ? null : row.GetString(4),
                    Contact = await row.IsDBNullAsync(5) ? null : row.GetString(5),
                    Dette = row.GetDecimal(10)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            throw;
        }
    }

    // ~ FAIRE UN CREDIT
    public static async Task<string> DemanderCreditAsync (string refClient, string codeAgence, string idEmploye, decimal montant)
    {
        if (montant <= 0) return "montant doit etre positif";
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
            //retrait depuit l'agence
            var compareSolde = new NpgsqlCommand("SELECT solde FROM agence WHERE code_agence = @codeAgence;", conn, transaction);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            if (((decimal?)await compareSolde.ExecuteScalarAsync() ?? 0m) < montant) return "solde agence insuffisant";

            var getCreditFromAgence = new NpgsqlCommand("UPDATE agence SET solde = solde - @montant WHERE code_agence = @codeAgence FOR UPDATE;", conn, transaction);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            if (await getCreditFromAgence.ExecuteNonQueryAsync() == 0)
            {
                await transaction.RollbackAsync();
                return "code agence missing";
            }

            const string query = @"INSERT INTO transactions (code_transaction, libelle, montant, refclient, code_agence, id_employe)
                                        VALUES (@codeTransaction, 'Credit', @montant, @refclient, @codeAgence, @idEmploye);";
            using var preparedQuery = new NpgsqlCommand(query, conn, transaction);
            preparedQuery.Parameters.AddWithValue("codeTransaction", codeTransaction);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
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
            Console.WriteLine(ex.Message);
            await transaction.RollbackAsync();
            return "error";
        }
    }

}
