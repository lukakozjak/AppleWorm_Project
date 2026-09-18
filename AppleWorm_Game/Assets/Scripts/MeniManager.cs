using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MeniManager : MonoBehaviour
{
    // Staticka promenljiva koja pamti stanje izmedju scena
    public static bool otvoriIzborNivoaNaStartu = false;

    [Header("UI Elementi")]
    public GameObject dugmeSolo;          // Referenca na dugme SOLO
    public GameObject dugme1v1;           // Referenca na dugme 1v1
    public GameObject logoNatpis;         // Referenca na "Crv Gari" logo iznad dugmadi
    public GameObject dugmeIzlaz;         // Referenca na dugme Izlaz
    public GameObject levelSelectPanel;   // Referenca na panel sa tabelom nivoa
    public GameObject dugmeGlavniMeniIzNivoa; // Dugme "Meni" gore desno, vidljivo samo dok je tabela nivoa otvorena

    [Header("1v1 - opciono")]
    public TMP_Text natpisKoBira1v1;          // Tekst iznad tabele nivoa, npr "Igrač 1 bira nivo" (moze ostati prazno)

    void Start()
    {
        // 1. Ako dolazimo iz nekog nivoa i zelimo odma prikaz izbora nivoa:
        if (otvoriIzborNivoaNaStartu)
        {
            if (dugmeSolo != null) dugmeSolo.SetActive(false);
            if (dugme1v1 != null) dugme1v1.SetActive(false);
            if (logoNatpis != null) logoNatpis.SetActive(false);
            if (dugmeIzlaz != null) dugmeIzlaz.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
            if (dugmeGlavniMeniIzNivoa != null) dugmeGlavniMeniIzNivoa.SetActive(true);

            // Resetujemo zastavicu na false da sledece pokretanje ne otvara automatski
            otvoriIzborNivoaNaStartu = false;
        }
        else
        {
            // 2. Standardno pokretanje glavnog menija iz pocetka
            if (dugmeSolo != null) dugmeSolo.SetActive(true);
            if (dugme1v1 != null) dugme1v1.SetActive(true);
            if (logoNatpis != null) logoNatpis.SetActive(true);
            if (dugmeIzlaz != null) dugmeIzlaz.SetActive(true);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (dugmeGlavniMeniIzNivoa != null) dugmeGlavniMeniIzNivoa.SetActive(false);
        }

        // 3. Ako je ovo otvaranje tabele nivoa u sklopu 1v1 biranja, prikazi natpis ko trenutno bira
        if (PodaciMeca.jeAktivno1v1Biranje)
        {
            if (natpisKoBira1v1 != null)
            {
                natpisKoBira1v1.gameObject.SetActive(true);
                string ime = (PodaciMeca.igracKojiTrenutnoBira == 1) ? PodaciMeca.imeIgraca1 : PodaciMeca.imeIgraca2;
                natpisKoBira1v1.text = ime + " bira nivo";
            }
        }
        else
        {
            if (natpisKoBira1v1 != null) natpisKoBira1v1.gameObject.SetActive(false);
        }
    }

    // Funkcija koju poziva dugme SOLO
    public void KliknutoSolo()
    {
        if (dugmeSolo != null) dugmeSolo.SetActive(false);
        if (dugme1v1 != null) dugme1v1.SetActive(false);
        if (logoNatpis != null) logoNatpis.SetActive(false);
        if (dugmeIzlaz != null) dugmeIzlaz.SetActive(false);
        if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        if (dugmeGlavniMeniIzNivoa != null) dugmeGlavniMeniIzNivoa.SetActive(true);
    }

    // Funkcija za novo dugme "1v1" u glavnom meniju
    public void Klinuto1v1()
    {
        SceneManager.LoadScene("Podesavanje1v1");
    }

    // Funkcija za novo dugme "Izlaz" u glavnom meniju
    public void IzadjiIzIgre()
    {
        Application.Quit();

#if UNITY_EDITOR
        // U Editoru Application.Quit() ne radi, pa rucno gasimo Play Mode radi testiranja
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Funkcija za novo dugme "Glavni meni" gore desno na tabeli nivoa -
    // radi ispravno i u SOLO tabeli i usred biranja nivoa za 1v1 (otkazuje to biranje)
    public void NazadNaGlavniEkran()
    {
        PodaciMeca.jeAktivno1v1Biranje = false;

        if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        if (dugmeGlavniMeniIzNivoa != null) dugmeGlavniMeniIzNivoa.SetActive(false);
        if (dugmeSolo != null) dugmeSolo.SetActive(true);
        if (dugme1v1 != null) dugme1v1.SetActive(true);
        if (logoNatpis != null) logoNatpis.SetActive(true);
        if (dugmeIzlaz != null) dugmeIzlaz.SetActive(true);
        if (natpisKoBira1v1 != null) natpisKoBira1v1.gameObject.SetActive(false);
    }

    // Funkcija koju poziva dugme 1 (ili bilo koje drugo dugme nivoa)
    public void OtvoriNivo(int brojNivoa)
    {
        // Ako smo trenutno u fazi biranja nivoa za 1v1 mec, ne ucitavamo nivo odmah -
        // prosledjujemo izbor menadzeru 1v1 podesavanja
        if (PodaciMeca.jeAktivno1v1Biranje)
        {
            Podesavanje1v1.ObradiIzborNivoa(brojNivoa, natpisKoBira1v1);
            return;
        }

        // Standardno SOLO ponasanje - ucitava scenu sa imenom "Nivo_" i prosledjenim brojem
        SceneManager.LoadScene("Nivo_" + brojNivoa);
    }
}   