using System;
using Npgsql;

namespace Bankmanaging.Models;

public interface IDatabaseConnection
{
    NpgsqlConnection Connected();
}

public sealed class DatabaseConnection : IDatabaseConnection
{
    private const string Owner = "Host=localhost;Database=bank;Username=bank_manager;Password=bankmanager";

    private static readonly Lazy<DatabaseConnection> _instance = new Lazy<DatabaseConnection>(() => new DatabaseConnection());
    
    public static DatabaseConnection Instance => _instance.Value;

    private DatabaseConnection() {}

    public NpgsqlConnection Connected()
    {
        return new NpgsqlConnection(Owner);
    }
}