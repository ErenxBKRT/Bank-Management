namespace Bankmanaging.Models;

public class Compte
{
    public string Numero { get; set; } = string.Empty;
    public decimal Solde { get; set; } = 0.00m;
    public decimal Credit { get; set; } = 0.00m;
    public bool Bloque { get; set; }
}
