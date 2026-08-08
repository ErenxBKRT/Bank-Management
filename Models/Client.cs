using System;

namespace Bankmanaging.Models
{
    public class Client 
    {
        public string IdClient { get; set; } = string.Empty;
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Adresse { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
        public bool Bloque { get; set; } 
    }
}