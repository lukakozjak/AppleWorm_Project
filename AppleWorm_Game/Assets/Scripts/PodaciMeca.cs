// Staticka klasa - pamti podatke o 1v1 mecu dok se prelazi izmedju scena
// (Podesavanje1v1 -> GlavniMeni levelSelectPanel -> Arena1v1)
public static class PodaciMeca
{
    public static string imeIgraca1 = "Igrač 1";
    public static string imeIgraca2 = "Igrač 2";

    public static bool istiNivo = true;
    public static bool nasumicanIzbor = false;

    public static int nivoIgraca1 = 1;
    public static int nivoIgraca2 = 1;

    // Kada je true, MeniManager.OtvoriNivo() ne ucitava nivo direktno,
    // nego prosledjuje izbor u Podesavanje1v1.ObradiIzborNivoa()
    public static bool jeAktivno1v1Biranje = false;
    public static int igracKojiTrenutnoBira = 1;
}