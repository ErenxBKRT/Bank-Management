using System;

namespace Bankmanaging.Models
{
    public class Transaction 
    {
        public string IdTransaction { get; set; } = string.Empty;
        public string Type {get; set; } = string.Empty;
        public string CodeAgence { get; set; } = string.Empty;
    }
}