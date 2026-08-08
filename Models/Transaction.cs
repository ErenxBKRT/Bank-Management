using System;

namespace Bankmanaging.Models;

public class Transaction
{
    public string Code { get; set; } = string.Empty;
    public string Libelle { get; set; } = string.Empty;
    public decimal Montant { get; set; } = 0.00m;
    public DateTime Date { get; set; }
    public string? Nom { get; set; }
    public string? CodeAgence { get; set; }
    public string? RefClient { get; set; }
}