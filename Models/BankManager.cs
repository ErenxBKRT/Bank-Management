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
    private const string Owner = "Host=localhost;Database=bank;Username=bank_manager;Password=bankmanager;Timeout=15";

    private static readonly Lazy<DatabaseConnection> _instance = new (()=> new DatabaseConnection());
    
    public static IDatabaseConnection Instance => _instance.Value;

    private DatabaseConnection() {}

    private static NpgsqlConnection Connected()
    {
        return new NpgsqlConnection(Owner);
    }
    
    public async Task<NpgsqlConnection> KaeruConnectAsync()
    {
        NpgsqlConnection connection = Connected();
        await connection.OpenAsync();
        return connection;
    }
}
