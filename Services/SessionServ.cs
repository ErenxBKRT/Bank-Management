namespace Bankmanaging.Services;

using Bankmanaging.Models;

public static class SessionServ
{
    // Stocke le compte client ou l'agence connecté(e)
    public static Compte? CurrentCompte { get; private set; }
    public static Agence? CurrentAgence { get; private set; }
    
    public static bool IsEmploye => CurrentAgence != null;
    public static bool IsClient => CurrentCompte != null;

    //ClientSession
    public static void StartClientSession(Compte compte)
    {
        CurrentCompte = compte;
        CurrentAgence = null;
    }

    //Agence session
    public static void StartEmployeSession(Agence agence)
    {
        CurrentAgence = agence;
        CurrentCompte = null;
    }

    //Delete session
    public static void ClearSession()
    {
        CurrentCompte = null;
        CurrentAgence = null;
    }
}