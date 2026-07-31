using Npgsql;


namespace BankManaging.Kaeru;

public interface IDepot
{
    
}

public sealed class Depot : IDepot
{
    private double solde;

    public async void Deposit (object? sender, RoutedEventArgs e)
    {
        try
        {
            IDatabaseConnection kaeru = new DatabaseConnection();
            using var deposing = kaeru.Connected();
            await deposing.OpenAsync();
            string query = "SELECT solde FROM clients WHERE "
            
        } catch (NpgsqlException ex)
        {
            
        }
    }
}

public interface IRetrait
{
    
}
