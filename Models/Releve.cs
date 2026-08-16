using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public class Releve
{
    private readonly string _numero = string.Empty;
    public decimal Solde { get; private set; }
    public int Virement { get; private set; }
    public int Retrait { get; private set; }
    public int Credit { get; private set; }
    public int Remboursement { get; private set; }
    public int Depot { get; private set; }


    public Releve (string numero)
    {
        _numero = numero;
    }

    public async Task GetSoldeAsync()
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new("SELECT SUM(solde) FROM agence WHERE numero = @numero;", kaeru);
            preparedQuery.Parameters.AddWithValue("numero", _numero);
            using NpgsqlDataReader solde = await preparedQuery.ExecuteReaderAsync();
            if (!await solde.ReadAsync())
            {
                Console.WriteLine("Numero incorrecte");
            }
            else if (await solde.ReadAsync())
            {
                Solde = solde.GetDecimal(0);
            }
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public async Task GetTotalTransactionAsync()
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();
        const string query = "SELECT COUNT(*) FROM transaction WHERE numero = @numero AND libelle = @libelle;";
        await using NpgsqlCommand preparedQuery = new(query, kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("numero", _numero);
        NpgsqlParameter libelle = preparedQuery.Parameters.AddWithValue("libelle", "");
        
        try
        {
            libelle.Value = "Virement";
            Virement = (int)(await preparedQuery.ExecuteScalarAsync() ?? 0);
            
            libelle.Value = "Retrait";
            Retrait = (int)(await preparedQuery.ExecuteScalarAsync() ?? 0);
            
            libelle.Value = "Credit";
            Credit = (int)(await preparedQuery.ExecuteScalarAsync() ?? 0);
            
            libelle.Value = "Remboursement";
            Remboursement = (int)(await preparedQuery.ExecuteScalarAsync() ?? 0);

            libelle.Value = "Depot";
            Depot = (int)(await preparedQuery.ExecuteScalarAsync() ?? 0);

            Console.WriteLine($"Virement {Virement}, Retrait {Retrait}, Credit {Credit}, Rembourse {Remboursement}, Depot {Depot}");
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
