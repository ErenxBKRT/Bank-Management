using System;

namespace Bankmanaging.Models;

public class Client 
{
    public string IdClient { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public decimal Solde;
    public string Pin { get; set; } = string.Empty;
    public DateTime DateCreation;
    public bool Bloque;
    public decimal Dette;
    public string IdEmploye { get; set; } = string.Empty;
    public string CodeAgence { get; set; } = string.Empty;
}
