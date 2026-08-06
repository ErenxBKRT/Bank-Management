using System;

namespace Bankmanaging.Models;

public class Agence
{
    public string CodeAgence { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public decimal Solde { get; set; }
    public DateTime Creation { get; set; }
}
