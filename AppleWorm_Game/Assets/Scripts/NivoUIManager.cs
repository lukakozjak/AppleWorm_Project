using UnityEngine;
using UnityEngine.SceneManagement;

public class NivoUIManager : MonoBehaviour
{
    [Header("UI Paneli")]
    public GameObject pausePanel;
    public GameObject victoryPanel;
    public GameObject svaNivoaZavrsenaPanel; // opciono - ekran "Čestitamo!" posle poslednjeg nivoa

    [Header("Nivoi")]
    public int ukupnoNivoa = 30;

    void Start()
    {
        // Vracamo brzinu igre na normalno i sakrivamo panele na pocetku
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (svaNivoaZavrsenaPanel != null) svaNivoaZavrsenaPanel.SetActive(false);
    }

    // --- LOGIKA ZA POBEDU I PAUZU ---
    public void PrikaziPobedu()
    {
        // Ako je ovo POSLEDNJI nivo, preskacemo obican "Pobeda" panel
        // i idemo pravo na ekran "Sva nivoa zavrsena"
        string trenutnoIme = SceneManager.GetActiveScene().name;
        string[] delovi = trenutnoIme.Split('_');

        if (delovi.Length == 2 && int.TryParse(delovi[1], out int trenutniBroj) && trenutniBroj >= ukupnoNivoa)
        {
            PrikaziSveZavrseno();
            return;
        }

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            Time.timeScale = 0f; // Zaustavlja igru
        }
    }

    public void OtvoriPauzu()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
            Time.timeScale = 0f; // Zaustavlja igru
        }
    }

    public void NastaviIgru()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1f; // Nastavlja igru
        }
    }

    // --- LOGIKA ZA DUGMAD ---
    public void RestartujNivo()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SledeciNivo()
    {
        Time.timeScale = 1f;

        string trenutnoIme = SceneManager.GetActiveScene().name; // npr. "Nivo_23"
        string[] delovi = trenutnoIme.Split('_');

        if (delovi.Length == 2 && int.TryParse(delovi[1], out int trenutniBroj))
        {
            if (trenutniBroj < ukupnoNivoa)
            {
                // Ucitava sledeci nivo PO IMENU, ne po poziciji u Build Settings -
                // ovo sprecava da nasumicno preskoci na Arena1v1/Podesavanje1v1 ili bilo sta drugo
                SceneManager.LoadScene("Nivo_" + (trenutniBroj + 1));
            }
            else
            {
                // Ovo je bio poslednji nivo
                PrikaziSveZavrseno();
            }
        }
        else
        {
            // Sigurnosna mreza - ako scena iz nekog razloga ne prati obrazac "Nivo_X"
            IdiUGlavniMeni();
        }
    }

    void PrikaziSveZavrseno()
    {
        if (svaNivoaZavrsenaPanel != null)
        {
            svaNivoaZavrsenaPanel.SetActive(true);
            Time.timeScale = 0f;
        }
        else
        {
            // Dok ne napravimo poseban ekran, bar sigurno ide u meni umesto da negde zaluta
            IdiUGlavniMeni();
        }
    }

    // Poziva se ISKLJUCIVO kada igrac klikne na dugme "Nivoi" / "Izbor Nivoa"
    public void IdiNaIzborNivoa()
    {
        Time.timeScale = 1f;
        MeniManager.otvoriIzborNivoaNaStartu = true;
        SceneManager.LoadScene("GlavniMeni");
    }

    // Poziva se kada igrac klikne na dugme "Glavni Meni" ili kada zavrsi poslednji nivo
    public void IdiUGlavniMeni()
    {
        Time.timeScale = 1f;
        MeniManager.otvoriIzborNivoaNaStartu = false;
        SceneManager.LoadScene("GlavniMeni");
    }
}   