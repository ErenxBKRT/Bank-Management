namespace Bankmanaging.Models;

public class Client 
{
    public int IdClient {get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
<<<<<<< HEAD
    public string? Contact { get; set; }
    public decimal Solde { get; set; } = 0.00m;
    public bool Bloque { get; set; } = false;
    public decimal Credit { get; set; } = 0.00m;
=======
    public string Contact { get; set; } = string.Empty;
    public bool Bloque { get; set; } = false;
>>>>>>> 1fc40be (...)
}
