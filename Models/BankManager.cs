using System;
using Npgsql;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public interface IDatabaseConnection
{
    Task<NpgsqlConnection> KaeruConnectAsync();
}

public sealed class DatabaseConnection : IDatabaseConnection
{
    private const string Owner = "Host=localhost;Database=bank;Username=bank_manager;Password=bankmanager";

    private static readonly Lazy<DatabaseConnection> _instance = new (()=> new DatabaseConnection());
    
    public static IDatabaseConnection Instance => _instance.Value;

    private DatabaseConnection() {}

    private static NpgsqlConnection Connected()
    {
        NpgsqlConnection Connected();
        string TestConnection();
    }
    
    public async Task<NpgsqlConnection> KaeruConnectAsync()
    {
         private const string Owner = "Host=localhost;Database=bank;Username=bank_manager;Password=bankmanager;Timeout=3;";

        //for antsa's pc
        //private const string Owner = "Host=localhost;Database=bank;Username=postgres;Password=root;Timeout=3;";

        private static readonly Lazy<DatabaseConnection> _instance = new Lazy<DatabaseConnection>(() => new DatabaseConnection());

        public static IDatabaseConnection Instance => _instance.Value;

        private DatabaseConnection() { }

        private static NpgsqlConnection Connected()
        {
            return new NpgsqlConnection(Owner);
        }

        public string TestConnection()
        {
            try
            {
                using var connection = Connected();
                connection.Open();

                return ("Connexion à la base de données réussie !");
            }
            catch (NpgsqlException ex)
            {
                return ($"Échec de connexion PostgreSQL : {ex.Message}");
            }
            catch (Exception ex)
            {
                return ($"Une erreur inattendue est survenue : {ex.Message}");
            }
        }
    }
}
