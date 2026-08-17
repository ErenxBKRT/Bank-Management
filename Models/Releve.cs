using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public class Releve
{
    private string _codeAgence;
    public decimal Solde { get; private set; }
    public int Virement { get; private set; }
    public int Credit { get; private set; }
    public int Remboursement { get; private set; }
    public int Depot { get; private set; }

    public Releve (string codeAgence)
    {
        _codeAgence = codeAgence;
    }

    public async Task GetTotalTransactionAsync()
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        await using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();
        const string query = "SELECT COUNT(*) FROM transaction WHERE code_agence = @code_agence AND libelle = @libelle;";
        await using NpgsqlCommand preparedQuery = new(query, kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("code_agence", _codeAgence);
        NpgsqlParameter libelle = preparedQuery.Parameters.AddWithValue("libelle", "");
        
        try
        {
            libelle.Value = "Virement";
            Virement = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());
            
            libelle.Value = "Credit";
            Credit = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());
            
            libelle.Value = "Remboursement";
            Remboursement = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());

            libelle.Value = "Depot";
            Depot = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync() ?? 0);

            //Console.WriteLine($"Virement {Virement}, Credit {Credit}, Rembourse {Remboursement}, Depot {Depot}");
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
