using Npgsql;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Bankmanaging.Models;

public static class ServiceEmploye
{
    // VIREMENT BANCAIRE
    public static async Task<bool> VirementBancaireAsync (decimal montant, string numero, string pin, string codeAgence, string nom)
    {
        if (montant <= 0) 
        {
            Console.WriteLine("montant negatif");
            return false;
        }

        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");
        
        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        bool eligible = await ServiceCarte.EstBloquerAsync(numero, kaeru);
        if (!eligible)
        {
            Console.WriteLine("le compte est bloquer");
            return false;
        }

        await using var kaeruTransac = await kaeru.BeginTransactionAsync();
        try 
        {
            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
            if (refClient == 0) return false;

            bool depot = await ServiceClient.DepositAsync(montant, refClient, kaeru, kaeruTransac);
            if (!depot)
            {
                await kaeruTransac.RollbackAsync();
                return false;
            }
            
            const string query = "INSERT INTO transaction (code, libelle, montant, nom, refclient, code_agence) VALUES (@code, 'Virement', @montant, @nom, @refClient, @codeAgence);";
            using var preparedQuery = new NpgsqlCommand(query, kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            
            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return true;
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    // ~ DEPOT BANCAIRE
    public static async Task<bool> DepotBancaireAsync (string numero, decimal montant, string codeAgence)
    {
        if (montant <= 0) return false;

        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using var kaeruTransac = await kaeru.BeginTransactionAsync();
        if (!await ServiceCarte.EstBloquerAsync(numero, kaeru, kaeruTransac))
        {
            Console.WriteLine("compte bloque");
            return false;
        }

        try
        {
            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
            if (refClient == 0)
            {
                Console.WriteLine("numero ou pin incorrect");
                return false;
            }

            if (!await ServiceClient.DepositAsync(montant, refClient, kaeru, kaeruTransac))
            {
                await kaeruTransac.RollbackAsync();
                return false;
            }

            const string query = "INSERT INTO transaction (code, libelle, montant, refclient) VALUES (@code, 'Depot', @montant, @refClient);";
            using var preparedQuery = new NpgsqlCommand(query, kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return true;
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    // ~ RETRAIT BANCAIRE
    public static async Task<bool> RetraitBancaireAsync (string numero, string pin, decimal montant, string codeAgence)
    {
        if (montant <= 0) return false;
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using var kaeruTransac = await kaeru.BeginTransactionAsync();
        if (!await ServiceCarte.EstBloquerAsync(numero, kaeru, kaeruTransac))
        {
            Console.WriteLine("compte bloque");
            return false;
        }

        try
        {
            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac, pin);
            if (refClient == 0) return false;

            if (!await ServiceClient.WithdrawAsync(montant, refClient, kaeru, kaeruTransac))
            {
                await kaeruTransac.RollbackAsync();
                return false;
            }

            const string query = "INSERT INTO transaction (code, libelle, montant, refclient) VALUES (@code, 'Retrait', @montant, @refClient);";
            using var preparedQuery = new NpgsqlCommand(query, kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return true;
        }
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine(ex.Message);
            return false;
        }
    }

    // ~ HISTORIQUE TRANSACTION
    public static async Task<List<Transaction>> HistoriqueTransactionAsync(string? idClient = null, string? libelle = null, string? codeAgence = null)
    {
        var listTransaction = new List<Transaction>();
        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        const string query = "SELECT * FROM transaction WHERE (refclient = @idClient OR @idClient IS NULL) AND (libelle = @libelle OR @libelle IS NULL) AND (code_agence = @codeAgence OR @codeAgence is NULL));";

        using var preparedQuery = new NpgsqlCommand(query, kaeru);
        preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("idClient", idClient ?? (object)DBNull.Value);
        preparedQuery.Parameters.AddWithValue("libelle", libelle ?? (object)DBNull.Value);
        try
        {
            using var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Transaction transaction = new()
                {
                    Code = row.GetString(0),
                    Libelle = row.GetString(1),
                    Montant = row.GetDecimal(2),
                    Date = row.GetDateTime(3),
                    Nom = await row.IsDBNullAsync(4) ? null : row.GetString(4),
                    CodeAgence = await row.IsDBNullAsync(5) ? null : row.GetString(5),
                    RefClient = row.GetString(6),
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
        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        const string query = "SELECT nom, prenom, adresse, contact FROM client WHERE credit > 0.00;";
        using var preparedQuery = new NpgsqlCommand(query, kaeru);
        var listClient = new List<Client>();

        try
        {
            var row = await preparedQuery.ExecuteReaderAsync();
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    Nom = row.GetString(0),
                    Prenom = await row.IsDBNullAsync(1) ? null : row.GetString(1),
                    Adresse = row.GetString(2),
                    Contact = row.GetString(3),
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
    public static async Task<bool> DemanderCreditAsync (string numero, string codeAgence, decimal montant)
    {
        if (montant <= 0)
        {
            Console.WriteLine("montant negatif");
            return false;
        }
        DateTime now = DateTime.Now;
        string microSecond = now.ToString("ffff");
        int randomNumber = RandomNumberGenerator.GetInt32(0, 100);
        string code = microSecond + "-" + randomNumber.ToString("D2");

        using var kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using var kaeruTransac = await kaeru.BeginTransactionAsync();
        try
        {
            //retrait depuit l'agence
            var compareSolde = new NpgsqlCommand("SELECT solde FROM agence WHERE code_agence = @codeAgence;", kaeru, kaeruTransac);
            compareSolde.Parameters.AddWithValue("codeAgence", codeAgence);
            if (((decimal?)await compareSolde.ExecuteScalarAsync() ?? 0m) < montant)
            {
                Console.WriteLine("solde agence insuffisant");
                return false;
            }

            var getCreditFromAgence = new NpgsqlCommand("UPDATE agence SET solde = solde - @montant WHERE code_agence = @codeAgence FOR UPDATE;", kaeru, kaeruTransac);
            getCreditFromAgence.Parameters.AddWithValue("codeAgence", codeAgence);
            getCreditFromAgence.Parameters.AddWithValue("montant", montant);
            if (await getCreditFromAgence.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                Console.WriteLine("code agence missing");
                return false;
            }

            int refClient = await ServiceCarte.GetIdAsync(numero, kaeru, kaeruTransac);
            if (refClient == 0) return false;

            const string query = "INSERT INTO transaction (code, libelle, montant, refclient, code_agence) VALUES (@code, 'Credit', @montant, @refClient, @codeAgence);";
            using var preparedQuery = new NpgsqlCommand(query, kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("code", code);
            preparedQuery.Parameters.AddWithValue("montant", montant);
            preparedQuery.Parameters.AddWithValue("refClient", refClient);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);

            await preparedQuery.ExecuteNonQueryAsync();
            await kaeruTransac.CommitAsync();
            return true;
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
            await kaeruTransac.RollbackAsync();
            return false;
        }
    }

}
