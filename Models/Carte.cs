namespace Bankmanaging.Models;

public class Carte
{
    public string Numero { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public decimal solde { get; set; } 
}
