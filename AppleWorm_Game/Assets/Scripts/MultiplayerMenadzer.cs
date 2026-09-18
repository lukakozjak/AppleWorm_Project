using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MultiplayerMenadzer : MonoBehaviour
{
    [Header("UI - Zajedničko")]
    public GameObject panelRezultata;
    public TMP_Text tekstRezultata;
    public GameObject panelOdbrojavanja;
    public TMP_Text tekstOdbrojavanja;
    public GameObject razdvojnica;
    public GameObject trakaGoreIgraca1;
    public GameObject trakaDoleIgraca1;
    public GameObject trakaGoreIgraca2;
    public GameObject trakaDoleIgraca2;

    [Header("UI - Igrač 1 (levo, WASD)")]
    public TMP_Text tekstImenaIgraca1;
    public TMP_Text tekstVremenaIgraca1;
    public GameObject panelCekanjaIgraca1;
    public GameObject panelMenijaIgraca1;
    public GameObject dugmeMeniIgraca1;
    public GameObject dugmeRestartIgraca1;

    [Header("UI - Igrač 2 (desno, strelice)")]
    public TMP_Text tekstImenaIgraca2;
    public TMP_Text tekstVremenaIgraca2;
    public GameObject panelCekanjaIgraca2;
    public GameObject panelMenijaIgraca2;
    public GameObject dugmeMeniIgraca2;
    public GameObject dugmeRestartIgraca2;

    // Koliko se drugi nivo pomera u stranu da se fizički ne bi preklapao sa prvim
    private readonly Vector3 pomakIgraca2 = new Vector3(1000f, 0f, 0f);

    private Scene sceneIgraca1;
    private Scene sceneIgraca2;
    private CrvKretanje crvIgraca1;
    private CrvKretanje crvIgraca2;

    private float vremeIgraca1 = 0f;
    private float vremeIgraca2 = 0f;
    private bool zavrsioIgrac1 = false;
    private bool zavrsioIgrac2 = false;
    private bool mecUToku = false;

    void Start()
    {
        if (tekstImenaIgraca1 != null) tekstImenaIgraca1.text = PodaciMeca.imeIgraca1;
        if (tekstImenaIgraca2 != null) tekstImenaIgraca2.text = PodaciMeca.imeIgraca2;

        if (panelRezultata != null) panelRezultata.SetActive(false);
        if (panelCekanjaIgraca1 != null) panelCekanjaIgraca1.SetActive(false);
        if (panelCekanjaIgraca2 != null) panelCekanjaIgraca2.SetActive(false);
        if (panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(false);
        if (panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(false);

        StartCoroutine(PokreniMec());
    }

    IEnumerator PokreniMec()
    {
        yield return UcitajIgraca("Nivo_" + PodaciMeca.nivoIgraca1, Vector3.zero, 0);
        yield return UcitajIgraca("Nivo_" + PodaciMeca.nivoIgraca2, pomakIgraca2, 1);

        // Blokiraj kretanje dok traje odbrojavanje
        if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(true);
        if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(true);

        yield return StartCoroutine(Odbrojavanje());

        // Odblokiraj tačno kad odbrojavanje završi i mec krene
        if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(false);
        if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(false);

        mecUToku = true;
    }

    // indeksIgraca: 0 = Igrač 1 (levo, WASD), 1 = Igrač 2 (desno, strelice)
    IEnumerator UcitajIgraca(string imeScene, Vector3 pomak, int indeksIgraca)
    {
        yield return SceneManager.LoadSceneAsync(imeScene, LoadSceneMode.Additive);

        // Bitno: uzimamo POSLEDNJE ucitanu scenu po redosledu, a ne po imenu -
        // ako oba igraca igraju ISTI nivo, GetSceneByName bi vratio pogresnu (ambiguity)
        Scene ucitanaScena = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        GameObject[] korenObjekti = ucitanaScena.GetRootGameObjects();

        CrvKretanje pronadjenCrv = null;
        Camera pronadjenaKamera = null;

        foreach (GameObject obj in korenObjekti)
        {
            obj.transform.position += pomak;

            if (pronadjenCrv == null) pronadjenCrv = obj.GetComponentInChildren<CrvKretanje>(true);
            if (pronadjenaKamera == null) pronadjenaKamera = obj.GetComponentInChildren<Camera>(true);

            // Gasimo Canvas (pauza/pobeda UI) i EventSystem iz nivoa - Arena1v1 ima svoje
            Canvas kanvas = obj.GetComponentInChildren<Canvas>(true);
            if (kanvas != null) kanvas.gameObject.SetActive(false);

            UnityEngine.EventSystems.EventSystem sistem = obj.GetComponentInChildren<UnityEngine.EventSystems.EventSystem>(true);
            if (sistem != null) sistem.gameObject.SetActive(false);
        }

        if (pronadjenaKamera != null)
        {
            pronadjenaKamera.rect = (indeksIgraca == 0)
                ? new Rect(0f, 0f, 0.5f, 1f)
                : new Rect(0.5f, 0f, 0.5f, 1f);

            // Kad se sirina kamere prepolovi, aspect ratio se prepolovi -
            // ovo poništava to sečenje i prikazuje ceo nivo umanjen, sa praznim prostorom iznad/ispod
            pronadjenaKamera.orthographicSize *= 2f;

            AudioListener slusalac = pronadjenaKamera.GetComponent<AudioListener>();
            if (slusalac != null && indeksIgraca == 1) slusalac.enabled = false;
        }

        if (pronadjenCrv != null)
        {
            pronadjenCrv.jeMultiplayer = true;
            pronadjenCrv.indeksIgraca = indeksIgraca;
            pronadjenCrv.mecMenadzer = this;

            if (indeksIgraca == 0)
            {
                pronadjenCrv.tasterGore = KeyCode.W;
                pronadjenCrv.tasterDole = KeyCode.S;
                pronadjenCrv.tasterLevo = KeyCode.A;
                pronadjenCrv.tasterDesno = KeyCode.D;
            }
            else
            {
                pronadjenCrv.tasterGore = KeyCode.UpArrow;
                pronadjenCrv.tasterDole = KeyCode.DownArrow;
                pronadjenCrv.tasterLevo = KeyCode.LeftArrow;
                pronadjenCrv.tasterDesno = KeyCode.RightArrow;
            }
        }

        if (indeksIgraca == 0) { sceneIgraca1 = ucitanaScena; crvIgraca1 = pronadjenCrv; }
        else { sceneIgraca2 = ucitanaScena; crvIgraca2 = pronadjenCrv; }
    }

    void PostaviVidljivostHUD(bool vidljivo)
    {
        if (razdvojnica != null) razdvojnica.SetActive(vidljivo);
        if (trakaGoreIgraca1 != null) trakaGoreIgraca1.SetActive(vidljivo);
        if (trakaDoleIgraca1 != null) trakaDoleIgraca1.SetActive(vidljivo);
        if (trakaGoreIgraca2 != null) trakaGoreIgraca2.SetActive(vidljivo);
        if (trakaDoleIgraca2 != null) trakaDoleIgraca2.SetActive(vidljivo);
    }

    IEnumerator Odbrojavanje()
    {
        PostaviVidljivostHUD(false);

        if (panelOdbrojavanja != null) panelOdbrojavanja.SetActive(true);

        string[] koraci = { "3", "2", "1", "KRENI!" };
        foreach (string korak in koraci)
        {
            if (tekstOdbrojavanja != null) tekstOdbrojavanja.text = korak;
            yield return new WaitForSecondsRealtime(0.8f);
        }

        if (panelOdbrojavanja != null) panelOdbrojavanja.SetActive(false);
        PostaviVidljivostHUD(true);
    }

    void Update()
    {
        if (!mecUToku) return;

        if (!zavrsioIgrac1)
        {
            vremeIgraca1 += Time.unscaledDeltaTime;
            if (tekstVremenaIgraca1 != null) tekstVremenaIgraca1.text = FormatirajVreme(vremeIgraca1);
        }

        if (!zavrsioIgrac2)
        {
            vremeIgraca2 += Time.unscaledDeltaTime;
            if (tekstVremenaIgraca2 != null) tekstVremenaIgraca2.text = FormatirajVreme(vremeIgraca2);
        }
    }

    string FormatirajVreme(float sekunde)
    {
        int minuti = Mathf.FloorToInt(sekunde / 60f);
        int sek = Mathf.FloorToInt(sekunde % 60f);
        int stotinke = Mathf.FloorToInt((sekunde * 100f) % 100f);
        return string.Format("{0:00}:{1:00}.{2:00}", minuti, sek, stotinke);
    }

    // Poziva CrvKretanje kada glava dodje do "Kraj" taga (indeksIgraca: 0 ili 1)
    public void IgracJeZavrsio(int indeksIgraca)
    {
        if (indeksIgraca == 0 && !zavrsioIgrac1)
        {
            zavrsioIgrac1 = true;
            if (panelCekanjaIgraca1 != null) panelCekanjaIgraca1.SetActive(true);
            if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(true);
            if (dugmeMeniIgraca1 != null) dugmeMeniIgraca1.SetActive(false);
            if (dugmeRestartIgraca1 != null) dugmeRestartIgraca1.SetActive(false);
            if (panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(false);
        }
        else if (indeksIgraca == 1 && !zavrsioIgrac2)
        {
            zavrsioIgrac2 = true;
            if (panelCekanjaIgraca2 != null) panelCekanjaIgraca2.SetActive(true);
            if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(true);
            if (dugmeMeniIgraca2 != null) dugmeMeniIgraca2.SetActive(false);
            if (dugmeRestartIgraca2 != null) dugmeRestartIgraca2.SetActive(false);
            if (panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(false);
        }

        if (zavrsioIgrac1 && zavrsioIgrac2) PrikaziRezultat();
    }

    void PrikaziRezultat()
    {
        mecUToku = false;
        PostaviVidljivostHUD(false);
        if (panelCekanjaIgraca1 != null) panelCekanjaIgraca1.SetActive(false);
        if (panelCekanjaIgraca2 != null) panelCekanjaIgraca2.SetActive(false);

        string pobednik;
        if (vremeIgraca1 < vremeIgraca2) pobednik = PodaciMeca.imeIgraca1 + " pobeđuje!";
        else if (vremeIgraca2 < vremeIgraca1) pobednik = PodaciMeca.imeIgraca2 + " pobeđuje!";
        else pobednik = "Nerešeno!";

        if (tekstRezultata != null)
        {
            tekstRezultata.text =
                PodaciMeca.imeIgraca1 + ": " + FormatirajVreme(vremeIgraca1) + "\n" +
                PodaciMeca.imeIgraca2 + ": " + FormatirajVreme(vremeIgraca2) + "\n\n" +
                pobednik;
        }

        if (panelRezultata != null) panelRezultata.SetActive(true);
    }

    // Kaci se na dugme "Odustani" u meniju svakog igraca, sa literalnim brojem 0 ili 1 kao parametrom u Inspector-u
    public void PredajSeIgrac(int indeksIgraca)
    {
        if (!mecUToku) return; // mec je vec zavrsen, ignorisi duplikat klika

        mecUToku = false;
        PostaviVidljivostHUD(false);
        if (panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(false);
        if (panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(false);
        if (panelCekanjaIgraca1 != null) panelCekanjaIgraca1.SetActive(false);
        if (panelCekanjaIgraca2 != null) panelCekanjaIgraca2.SetActive(false);
        if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(true);
        if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(true);
        if (dugmeMeniIgraca1 != null) dugmeMeniIgraca1.SetActive(false);
        if (dugmeMeniIgraca2 != null) dugmeMeniIgraca2.SetActive(false);

        string imePredao = (indeksIgraca == 0) ? PodaciMeca.imeIgraca1 : PodaciMeca.imeIgraca2;
        string imePobednika = (indeksIgraca == 0) ? PodaciMeca.imeIgraca2 : PodaciMeca.imeIgraca1;

        if (tekstRezultata != null)
        {
            tekstRezultata.text = imePredao + " je odustao.\n" + imePobednika + " pobeđuje!";
        }

        if (panelRezultata != null) panelRezultata.SetActive(true);
    }

    // Ova dva dugmeta se kace na Restart dugmad koja se pojave kad neko od igraca pogine
    // Kaci se na dugme "Meni" i "Nastavi" umesto direktnog GameObject.SetActive -
    // ovako i pokretanje crva blokiramo/odblokiramo u istom koraku
    public void OtvoriMeniIgraca1()
    {
        if (panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(true);
        if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(true);
    }

    public void ZatvoriMeniIgraca1()
    {
        if (panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(false);
        if (crvIgraca1 != null) crvIgraca1.PostaviBlokiranjeMenija(false);
    }

    public void OtvoriMeniIgraca2()
    {
        if (panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(true);
        if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(true);
    }

    public void ZatvoriMeniIgraca2()
    {
        if (panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(false);
        if (crvIgraca2 != null) crvIgraca2.PostaviBlokiranjeMenija(false);
    }

    private bool restartUTokuIgraca1 = false;
    private bool restartUTokuIgraca2 = false;

    public void RestartujIgraca1()
    {
        if (restartUTokuIgraca1) return; // vec je u toku restart, ignorisi dodatne klikove
        if (zavrsioIgrac1) return;       // ovaj igrac je vec zavrsio - restart se vise ne dozvoljava
        StartCoroutine(ObavijRestart1());
    }

    IEnumerator ObavijRestart1()
    {
        restartUTokuIgraca1 = true;
        yield return PonovoUcitajIgraca(sceneIgraca1, "Nivo_" + PodaciMeca.nivoIgraca1, Vector3.zero, 0);
        restartUTokuIgraca1 = false;
    }

    public void RestartujIgraca2()
    {
        if (restartUTokuIgraca2) return;
        if (zavrsioIgrac2) return;
        StartCoroutine(ObavijRestart2());
    }

    IEnumerator ObavijRestart2()
    {
        restartUTokuIgraca2 = true;
        yield return PonovoUcitajIgraca(sceneIgraca2, "Nivo_" + PodaciMeca.nivoIgraca2, pomakIgraca2, 1);
        restartUTokuIgraca2 = false;
    }

    IEnumerator PonovoUcitajIgraca(Scene staraScena, string imeScene, Vector3 pomak, int indeksIgraca)
    {
        if (indeksIgraca == 0 && panelMenijaIgraca1 != null) panelMenijaIgraca1.SetActive(false);
        if (indeksIgraca == 1 && panelMenijaIgraca2 != null) panelMenijaIgraca2.SetActive(false);

        if (staraScena.IsValid())
        {
            yield return SceneManager.UnloadSceneAsync(staraScena);
        }

        yield return UcitajIgraca(imeScene, pomak, indeksIgraca);
    }

    // Kace se na dugmad na ekranu rezultata
    // Kaci se na dugme "Novi mec" na ekranu rezultata - ide pravo na podesavanje, ne kroz Glavni meni
    public void NoviMec()
    {
        SceneManager.LoadScene("Podesavanje1v1");
    }

    public void GlavniMeni()
    {
        SceneManager.LoadScene("GlavniMeni");
    }
}