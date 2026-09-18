using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Podesavanje1v1 : MonoBehaviour
{
    [Header("Unos imena")]
    public TMP_InputField poljeImeIgraca1;
    public TMP_InputField poljeImeIgraca2;

    [Header("Izbor nacina (koristi Toggle komponente)")]
    public Toggle prekidacIstiNivo;     // ukljuceno = isti nivo za oba, iskljuceno = razliciti nivoi
    public Toggle prekidacSamBiram;     // ukljuceno = igraci sami biraju, iskljuceno = nasumicno

    // Poziva dugme "Dalje" na ekranu za podesavanje
    public void KliknutoDalje()
    {
        PodaciMeca.imeIgraca1 = string.IsNullOrEmpty(poljeImeIgraca1.text) ? "Igrač 1" : poljeImeIgraca1.text;
        PodaciMeca.imeIgraca2 = string.IsNullOrEmpty(poljeImeIgraca2.text) ? "Igrač 2" : poljeImeIgraca2.text;

        PodaciMeca.istiNivo = prekidacIstiNivo.isOn;
        PodaciMeca.nasumicanIzbor = !prekidacSamBiram.isOn;

        if (PodaciMeca.nasumicanIzbor)
        {
            PodaciMeca.nivoIgraca1 = Random.Range(1, 31); // 1 do 30
            PodaciMeca.nivoIgraca2 = PodaciMeca.istiNivo ? PodaciMeca.nivoIgraca1 : Random.Range(1, 31);
            SceneManager.LoadScene("Arena1v1");
        }
        else
        {
            // Sam biram - vracamo se u GlavniMeni i "posudjujemo" njegovu vec postojecu tabelu nivoa,
            // istim trikom kojim se ona vec otvara kad se igrac vrati iz nivoa (otvoriIzborNivoaNaStartu)
            PodaciMeca.jeAktivno1v1Biranje = true;
            PodaciMeca.igracKojiTrenutnoBira = 1;
            MeniManager.otvoriIzborNivoaNaStartu = true;
            SceneManager.LoadScene("GlavniMeni");
        }
    }

    // Poziva dugme "Glavni meni" na ekranu za podesavanje
    public void KliknutoGlavniMeni()
    {
        SceneManager.LoadScene("GlavniMeni");
    }

    // Poziva se iz MeniManager.OtvoriNivo() kada je PodaciMeca.jeAktivno1v1Biranje == true
    public static void ObradiIzborNivoa(int brojNivoa, TMP_Text natpisKoBira)
    {
        if (PodaciMeca.igracKojiTrenutnoBira == 1)
        {
            PodaciMeca.nivoIgraca1 = brojNivoa;

            if (PodaciMeca.istiNivo)
            {
                // Oba igraca igraju isti nivo - gotovi smo sa biranjem
                PodaciMeca.nivoIgraca2 = brojNivoa;
                PodaciMeca.jeAktivno1v1Biranje = false;
                SceneManager.LoadScene("Arena1v1");
            }
            else
            {
                // Red je na drugog igraca da izabere svoj nivo - panel ostaje otvoren
                PodaciMeca.igracKojiTrenutnoBira = 2;
                if (natpisKoBira != null) natpisKoBira.text = PodaciMeca.imeIgraca2 + " bira nivo";
            }
        }
        else
        {
            PodaciMeca.nivoIgraca2 = brojNivoa;
            PodaciMeca.jeAktivno1v1Biranje = false;
            SceneManager.LoadScene("Arena1v1");
        }
    }
}