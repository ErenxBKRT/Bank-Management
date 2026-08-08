using Npgsql;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bankmanaging.Models;
public static class ServiceClient
{
    public static async Task<string> AddAsync (string nom, string adresse, string contact, string? prenom = null)
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
            return "Ajout du nouveau client terminé avec succès.";
        } 
        catch (NpgsqlException ex) 
        {
            Debug.WriteLine($"Error : {ex.Message}"); 
            return "L'ajout du nouveau client a échoué.";
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Error : {ex.Message}"); 
            return "L'ajout du nouveau client a échoué.";
        }
    }

    public static async Task<string> UpdateAsync (int idClient, string nom, string adresse, string contact, string? prenom = null)
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

            if(await preparedQuery.ExecuteNonQueryAsync() == 0) return "L'identifiant du client est incorrect."; 
            return "Les informations du client mis à jour avec succès.";
        } 
        catch (NpgsqlException ex) 
        {
            Debug.WriteLine($"Error : {ex.Message}"); 
            return "Le mis à jour des informations a echoué.";
        }
        catch (Exception ex) 
        {
            Debug.WriteLine($"Error : {ex.Message}"); 
            return "Le mis à jour des informations a echoué.";
        }
    }

    public static async Task<string> LockAsync (bool bloque, int idClient)
    {
        using NpgsqlConnection kaeru = await DatabaseConnection.Instance.KaeruConnectAsync();
        using NpgsqlTransaction kaeruTransac = await kaeru.BeginTransactionAsync();

        try 
        {
            using NpgsqlCommand preparedQuery = new ("UPDATE client SET bloque = @bloque WHERE id_client = @idClient;", kaeru, kaeruTransac);
            preparedQuery.Parameters.AddWithValue("bloque", bloque);
            preparedQuery.Parameters.AddWithValue("idClient", idClient);

            if (await preparedQuery.ExecuteNonQueryAsync() == 0)
            {
                await kaeruTransac.RollbackAsync();
                return "L'identifiant du client est incorrect.";
            }

            await ServiceCompte.LockAsync(true, idClient);
            await kaeruTransac.CommitAsync();
            return "Le client a été bloqué avec succès";
        } 
        catch (NpgsqlException ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "La requête pour bloquer le client a échoué.";
        }
        catch (Exception ex)
        {
            await kaeruTransac.RollbackAsync();
            Debug.WriteLine($"Error : {ex.Message}");
            return "La requête pour bloquer le client a échoué.";
        }
    }

    public static async Task<string> VerifyAsync (int idClient, NpgsqlConnection kaeru, NpgsqlTransaction? kaeruTransac = null)
    {
        using NpgsqlCommand preparedQuery = new ("SELECT * FROM client WHERE id_client = @idClient;", kaeru, kaeruTransac);
        preparedQuery.Parameters.AddWithValue("idClient", idClient);

        if (await preparedQuery.ExecuteNonQueryAsync() == 0)
        {
            return "L'identifiant du client est incorrect.";
        }
        return "VERIFIED";
    }

}