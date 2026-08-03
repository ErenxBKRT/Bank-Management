using System;

namespace Bankmanaging.Models;

public class Transactions
{
    public string CodeTransaction { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal Montant { get; set; } = 0.00m;
    public DateTime DateTransaction { get; set; }
    public string? Nom { get; set; }
    public string? IdEmploye { get; set; }
    public string? CodeAgence { get; set; }
    public string? RefClient { get; set; }
}
