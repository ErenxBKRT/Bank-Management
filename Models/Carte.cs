using System;

namespace Bankmanaging.Models;

public class Carte
{
    public string Numero { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Pin { get; set; } = string.Empty;
    public string RefClient { get; set; } = string.Empty;
}
