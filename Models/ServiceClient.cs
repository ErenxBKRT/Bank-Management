using Npgsql;
using System;
using System.Threading.Tasks;

namespace Bankmanaging.Models;
public static class ServiceClient
{
    public static async Task<Result> AddAsync (string nom, string adresse, string contact, string? prenom = null)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try 
        {
            using NpgsqlCommand preparedQuery = new ("INSERT INTO client (nom, prenom, adresse, contact) VALUES (@nom, @prenom, @adresse, @contact);", kaeru);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("prenom", prenom ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("adresse", adresse);
            preparedQuery.Parameters.AddWithValue("contact", contact);
            await preparedQuery.ExecuteNonQueryAsync();

            return new (true, "Ajout du nouveau client terminé avec succès.");
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine($"Error : {ex.Message}"); 
            return new (false, "L'ajout du nouveau client a échoué.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error : {ex.Message}"); 
            return new (false, "L'ajout du nouveau client a échoué.");
        }
    }

    public static async Task<Result> UpdateAsync (int idClient, string nom, string adresse, string contact, string? prenom = null)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();

        try 
        {
            using NpgsqlCommand preparedQuery = new ("UPDATE client SET nom = @nom, prenom = @prenom, adresse = @adresse, contact = @contact WHERE id_client = @idClient;", kaeru);
            preparedQuery.Parameters.AddWithValue("nom", nom);
            preparedQuery.Parameters.AddWithValue("prenom", prenom ?? (object)DBNull.Value);
            preparedQuery.Parameters.AddWithValue("adresse", adresse);
            preparedQuery.Parameters.AddWithValue("contact", contact);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            if(await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                return new (false, "L'identifiant du client est incorrect.");
            }
            return new (true, "Les informations du client mis à jour avec succès.");
        } 
        catch (NpgsqlException ex) 
        {
            Console.WriteLine($"Error : {ex.Message}"); 
            return new (false, "Le mis à jour des informations a echoué.");
        }
        catch (Exception ex) 
        {
            Console.WriteLine($"Error : {ex.Message}"); 
            return new (false, "Le mis à jour des informations a echoué.");
        }
    }

    public static async Task<Result> LockAsync (bool bloque, int idClient)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            using NpgsqlCommand preparedQuery = new ("UPDATE client SET bloquer = @bloque WHERE id_client = @idClient;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("bloque", bloque);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return new (false, "L'identifiant du client est incorrect.");
            }
            if (bloque == true)
            {
                await ServiceCompte.LockAsync(true, idClient);
            }
            
            await kaeruTransac.CommitAsync();
            if (bloque == false)
            {
                return new (true, "Le client a été debloqué avec succès");
            }
            return new (true, "Le client a été bloqué avec succès");
        } 
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "La requête pour bloquer le client a échoué.");
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Console.WriteLine($"Error : {ex.Message}");
            return new (false, "La requête pour bloquer le client a échoué.");
        }
    }

    public static async Task<Result> VerifyAsync (int idClient, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM client WHERE id_client = @idClient FOR UPDATE;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        if (await preparedQuery.ExecuteScalarAsync() == null)
        {
            return new (false, "L'identifiant du client est incorrect.");
        }
        return new (true, "Connection réussie");
    }

}