using System;

namespace Bankmanaging.Models;

public class Client 
{
    public string IdClient { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public string Adresse { get; set; } = string.Empty;
    public string? Mail { get; set; }
    public string? Contact { get; set; }
    public decimal Solde = 0.00m;
    public DateTime DateCreation;
    public bool Bloque = false;
    public decimal Dette = 0.00m;
    public string? IdEmploye { get; set; }
    public string? CodeAgence { get; set; }
}
