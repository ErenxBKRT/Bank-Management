using Npgsql;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;

namespace Bankmanaging.Models
{
    public static class ServiceClient
    {
        // ~INSERT THE NEW CLIENT INTO THE DATABASE 
        public static async Task<string> CreateClientAsync (Client client)
        {
            DateTime now = DateTime.Now;
            string microSecond = now.ToString("ffff");
            int randomNumber = RandomNumberGenerator.GetInt32(0, 1000);
            client.IdClient = "C" + microSecond + randomNumber.ToString("D3");

            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = @"INSERT INTO clients (id_client, nom, prenom, adresse, mail, contact, solde, pin)
                                VALUES (@id, @nom, @prenom, @adresse, @mail, @contact, @solde, @pin);";

            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("id", client.IdClient);
            preparedQuery.Parameters.AddWithValue("nom", client.Nom);
            preparedQuery.Parameters.AddWithValue("prenom", client.Prenom);
            preparedQuery.Parameters.AddWithValue("adresse", client.Adresse);
            preparedQuery.Parameters.AddWithValue("mail", client.Mail);
            preparedQuery.Parameters.AddWithValue("contact", client.Contact);
            preparedQuery.Parameters.AddWithValue("solde", client.Solde);
            preparedQuery.Parameters.AddWithValue("pin", client.Pin);

            try 
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            } 
            catch (NpgsqlException ex) 
            {
                Console.WriteLine(ex); // temporary!
                return "error";
            }
        }

        // ~READJUST/UPDATE CLIENT INFORMATION
        public static async Task<string> UpdateClientAsync (Client client)
        {
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = @"UPDATE clients 
                                SET nom = @nom, prenom = @prenom, adresse = @adresse, mail = @mail, contact = @contact
                                WHERE id_client = @idClient;";

            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("nom", client.Nom);
            preparedQuery.Parameters.AddWithValue("prenom", client.Prenom);
            preparedQuery.Parameters.AddWithValue("adresse", client.Adresse);
            preparedQuery.Parameters.AddWithValue("mail", client.Mail);
            preparedQuery.Parameters.AddWithValue("contact", client.Contact);
            preparedQuery.Parameters.AddWithValue("idClient", client.IdClient);

            try 
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            } 
            catch (NpgsqlException ex) 
            {
                Console.WriteLine(ex); // temporary!
                return "error";
            }
        }

        // ~CHANGE PIN!!
        public static async Task<string> ChangePinAsync (string newPin, string idClient)
        {
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = "UPDATE clients SET pin = @pin WHERE id_client = @idClient;";

            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("pin", newPin);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            try 
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            } 
            catch (NpgsqlException ex) 
            {
                Console.WriteLine(ex); // temporary!
                return "error";
            }
        }

        // ~LOCK/UNLOCK A CLIENT ACCOUNT
        public static async Task<string> LockClientAsync (bool bloque, string idClient, string idEmploye, string codeAgence)
        {
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = @"UPDATE clients 
                                SET bloque = @bloque, id_employe = @idEmploye, code_agence = @codeAgence 
                                WHERE id_client = @idClient;";

            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("bloque", bloque);
            preparedQuery.Parameters.AddWithValue("idEmploye", idEmploye);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            try 
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            } 
            catch (NpgsqlException ex) 
            {
                Console.WriteLine(ex); // temporary!
                return "error";
            }
        }

        // ~USE TO DO A TRANSACTION
        public static async Task<string> DepositAsync (decimal amount, string idClient)
        {
            if (amount <= 0) return "amount should be positif";
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = "UPDATE clients SET solde = solde + @amount WHERE id_client = @idClient;";
            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("amount", amount);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            try
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex); // temporary!
                return "error";
            }
        }
        public static async Task<string> WithdrawAsync (decimal amount, string idClient)
        {
            if (amount <= 0) return "amount should be positif";
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = "UPDATE clients SET solde = solde - @amount WHERE id_client = @idClient AND solde >= @amount;";
            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("amount", amount);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            try
            {
                await preparedQuery.ExecuteNonQueryAsync();
                return "success";
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex); // temporary!;
                return "error";
            }
        }
        
        // ~CONSULTER SOLDE
        public static async Task<decimal> ConsulterSoldeAsync (string idClient)
        {
            IDatabaseConnection kaeru = DatabaseConnection.Instance;
            using var conn = kaeru.Connected();
            await conn.OpenAsync();

            const string query = "SELECT solde FROM clients WHERE id_client = @idClient;";
            using var preparedQuery = new NpgsqlCommand(query, conn);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            try
            {
                object? solde = await preparedQuery.ExecuteScalarAsync();
                if (solde == null || solde == DBNull.Value)
                {
                    throw new InvalidOperationException("Client non existant");
                }
                return (solde as decimal?) ?? 0.00m;
            }
            catch (NpgsqlException ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

    }
}