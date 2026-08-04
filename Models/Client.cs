namespace Bankmanaging.Models;

public class Client 
{
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public decimal Solde { get; set; } = 0.00m;
    public bool Bloque { get; set; } = false;
    public decimal Credit { get; set; } = 0.00m;
}
