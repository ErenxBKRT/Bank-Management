using System;

namespace Bankmanaging.Models;

public class Employe
{
    public string IdEmploye { get; set; } = string.Empty;
    public string Nom { get; set; } = string.Empty;
    public string? Prenom { get; set; }
    public DateTime DateCreation;
    public string CodeAgence { get; set; } = string.Empty;
}
