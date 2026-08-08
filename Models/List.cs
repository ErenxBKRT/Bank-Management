using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Bankmanaging.Models;

public static class ManageTransaction
{
    public static async Task<IEnumerable<Client>> ListClientAsync (int? idClient = null, bool? bloque = null, string? nom = null)
    {
        List<Client> listClient = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try 
        {
            using NpgsqlCommand preparedQuery = new ("SELECT * FROM client WHERE (id_client = @idClient OR @idClient IS NULL) AND (bloque = @bloque OR @bloque IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
            preparedQuery.Parameters.AddWithValue("bloque", bloque ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("idClient", idClient ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();
        
            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    Id = row.GetInt32(0),
                    Nom = row.GetString(1),
                    Prenom = row.IsDBNull(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Contact = row.GetString(4),
                    Bloque = row.GetBoolean(6)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

    public static async Task<IEnumerable<Transaction>> HistoriqueTransactionAsync(string? numero = null, string? libelle = null, string? codeAgence = null)
    {
        List<Transaction> listTransaction = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        
        try
        {

            using NpgsqlCommand preparedQuery = new ("SELECT * FROM transaction WHERE (num_compte = @numero OR @numero IS NULL) AND (libelle = @libelle OR @libelle IS NULL) AND (code_agence = @codeAgence OR @codeAgence is NULL));", kaeru);
            preparedQuery.Parameters.AddWithValue("codeAgence", codeAgence ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("libelle", libelle ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();

            while (await row.ReadAsync())
            {
                Transaction transaction = new()
                {
                    Code = row.GetString(0),
                    Libelle = row.GetString(1),
                    Montant = row.GetDecimal(2),
                    Date = row.GetDateTime(3),
                    Nom = await row.IsDBNullAsync(4) ? null : row.GetString(4),
                    CodeAgence = await row.IsDBNullAsync(5) ? null : row.GetString(5),
                    Numero = row.GetString(6),
                    Descritpion = await row.IsDBNullAsync(7) ? null : row.GetString(7)
                };
                listTransaction.Add(transaction);
            }
            return listTransaction;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

    public static async Task<IEnumerable<Client>> ListClientCreditAsync (string? nom = null)
    {
        List<Client> listClient = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {

            using NpgsqlCommand preparedQuery = new ("SELECT * date FROM client WHERE credit > 0.00 AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
            preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();

            while (await row.ReadAsync())
            {
                Client client = new()
                {
                    Id = row.GetInt32(0),
                    Nom = row.GetString(1),
                    Prenom = await row.IsDBNullAsync(2) ? null : row.GetString(2),
                    Adresse = row.GetString(3),
                    Contact = row.GetString(4),
                    Bloque = row.GetBoolean(5)
                };
                listClient.Add(client);
            }
            return listClient;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

    public static async Task<IEnumerable<Carte>> ListCarteAsync (string? numero = null, string? nom = null)
    {
        List<Carte> listCard = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("SELECT compte.numero, client.nom, client.prenom FROM compte JOIN client ON numero.refclient = client.id_client WHERE (num_compte = @numero OR @numero IS NULL) AND (nom LIKE @nom OR prenom LIKE @nom OR @nom IS NULL);", kaeru);
            preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("nom", nom ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();

            while (await row.ReadAsync())
            {
                Carte card = new()
                {
                    Numero = row.GetString(0),
                    Nom = row.GetString(1),
                    Prenom = await row.IsDBNullAsync(2) ? null : row.GetString(2)
                };
                listCard.Add(card);
            }
            return listCard;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

    public static async Task<IEnumerable<Compte>> ListCompteAsync (string? numero = null, string? nom = null)
    {
        List<Compte> listCompte = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("SELECT numero, solde, credit, bloquer FROM compte WHERE numero = @numero OR @numero IS NULL;", kaeru);
            preparedQuery.Parameters.AddWithValue("numero", numero ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();

            while (await row.ReadAsync())
            {
                Compte compte = new()
                {
                    Numero = row.GetString(0),
                    Solde = row.GetDecimal(1),
                    Credit = row.GetDecimal(2),
                    Bloque = row.GetBoolean(3)
                };
                listCompte.Add(compte);
            }
            return listCompte;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

    public static async Task<IEnumerable<Agence>> ListAgenceAsync (string? code)
    {
        List<Agence> listAgence = [];
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try
        {
            using NpgsqlCommand preparedQuery = new ("SELECT * FROM agence WHERE code_agence = @code OR @code IS NULL;", kaeru);
            preparedQuery.Parameters.AddWithValue("code", code ?? (object)DBNull.Value);
            using NpgsqlDataReader row = await preparedQuery.ExecuteReaderAsync();

            while (await row.ReadAsync())
            {
                Agence agence = new()
                {
                    CodeAgence = row.GetString(0),
                    Adresse = row.GetString(1),
                    Solde = row.GetDecimal(2)
                };
                listAgence.Add(agence);
            }
            return listAgence;
        }
        catch (NpgsqlException ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
            return [];
        }
    }

}
