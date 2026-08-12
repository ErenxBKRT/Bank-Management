using System;
using Npgsql;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public record Result (bool Status, string Message); // la valeur de retour de chaque methode dans le modele a part les list dans Listing.cs
public record LoginAccountClient (bool Status, string Message, Compte? CompteClient = null); // valeur pour le login client seulement
public record LoginAccountAgence (bool Status, string Message, Agence? CompteAgence = null); // valeur pour le login Agence seulement


public interface IDatabaseConnection
{
    Task<NpgsqlConnection> KaeruConnectAsync();
}

public sealed class DatabaseConnection : IDatabaseConnection
{
    private const string Owner = "Host=localhost;Database=bank;Username=postgres;Password=root;Timeout=15";

    //private const string Owner = "Host=localhost;Database=bank;Username=manager;Password=manager;Timeout=15";

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
