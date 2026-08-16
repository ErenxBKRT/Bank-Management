using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;

public class Releve
{
    private string _codeAgence;
    public decimal Solde { get; private set; }
    public int Virement { get; private set; }
    public int Retrait { get; private set; }
    public int Credit { get; private set; }
    public int Remboursement { get; private set; }
    public int Depot { get; private set; }


    public Releve (string codeAgence)
    {
        _codeAgence = codeAgence;
    }

    public async Task GetSoldeAsync()
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new("SELECT SUM(solde) FROM agence WHERE code_agence = @codeAgence;", kaeru);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence);
            using NpgsqlDataReader solde = await preparedQuery.ExecuteReaderAsync();
            if (!await solde.ReadAsync())
            {
                return;
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
            Virement = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());
            
            libelle.Value = "Retrait";
            Retrait = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());
            
            libelle.Value = "Credit";
            Credit = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());
            
            libelle.Value = "Remboursement";
            Remboursement = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync());

            libelle.Value = "Depot";
            Depot = Convert.ToInt32(await preparedQuery.ExecuteScalarAsync() ?? 0);

            // Console.WriteLine($"Virement {Virement}, Retrait {Retrait}, Credit {Credit}, Rembourse {Remboursement}, Depot {Depot}");
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
