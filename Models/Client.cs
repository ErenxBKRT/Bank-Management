namespace Bankmanaging.Models;

public class Client 
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public string? Contact { get; set; }
    public bool Bloque { get; set; } = false;
    public decimal Credit { get; set; } = 0.00m;
}
