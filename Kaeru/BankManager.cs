using Npgsql;

namespace BankManaging.Kaeru;

public interface IDatabaseConnection
{
    NpgsqlConnection Connected();
}

public sealed class DatabaseConnection : IDatabaseConnection
{
    private const string Owner = "Host=localhost;Database=bank;Username=bank_manager;Password=bankmanager";

    public NpgsqlConnection Connected()
    {
        return new NpgsqlConnection(Owner);
    }
}