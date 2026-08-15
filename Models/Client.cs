namespace Bankmanaging.Models;

public class Client 
{
    public int Id {get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Prenom { get; set; } = string.Empty;
    public string Adresse { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public bool Bloque { get; set; } = false;
    public string StatusText => Bloque? "Bloque":"Actif";
    public string StatusBtn => Bloque? "Debloquer":"Bloquer";
}
