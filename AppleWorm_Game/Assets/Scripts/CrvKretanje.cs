using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrvKretanje : MonoBehaviour
{
    public enum SmerKretanja { Levo, Desno, Gore, Dole }

    [Header("Početna Podešavanja Nivoa")]
    public SmerKretanja pocetniSmerGledanja = SmerKretanja.Desno;
    public int pocetnaDuzinaTela = 2;

    [Header("Postavke Animacije i Kretanja")]
    public float trajanjeKoraka = 0.12f;
    public GameObject teloPrefab;

    [Header("Granica za Ponor")]
    public float granicaIspadanja = -15f;

    [Header("Multiplayer podešavanja - ne diraj u SOLO nivoima")]
    public bool jeMultiplayer = false;
    public int indeksIgraca = 0; // 0 = Igrač 1 (WASD), 1 = Igrač 2 (strelice)
    public MultiplayerMenadzer mecMenadzer;
    public KeyCode tasterGore = KeyCode.UpArrow;
    public KeyCode tasterDole = KeyCode.DownArrow;
    public KeyCode tasterLevo = KeyCode.LeftArrow;
    public KeyCode tasterDesno = KeyCode.RightArrow;

    // Poziva se kada crv umre - MultiplayerMenadzer se prijavljuje na ovo da prikaze dugme Restart
    public System.Action OnSmrt;

    private List<Transform> deloviTela = new List<Transform>();
    private Vector2Int trenutniSmer = Vector2Int.right;
    private bool jeUKretanju = false;
    private bool jeMrtav = false;

    // Veličina kutije za proveru polja
    private Vector2 velicinaKutije = new Vector2(0.7f, 0.7f);

    void Start()
    {
        deloviTela.Add(transform);
        PostaviPocetniSmer();
        KreirayPocetnoTelo();
        Physics2D.SyncTransforms();
        StartCoroutine(ProveriPocetnuGravitaciju());
    }

    // Poziva MultiplayerMenadzer kad se otvori/zatvori meni za ovog igraca -
    // dok je true, tasteri se ignorisu, ali pad/fizika i dalje rade normalno
    private bool jeMenijOtvoren = false;
    public void PostaviBlokiranjeMenija(bool blokirano)
    {
        jeMenijOtvoren = blokirano;
    }

    void Update()
    {
        if (jeUKretanju || jeMrtav || jeMenijOtvoren) return;

        if (jeMultiplayer)
        {
            if (Input.GetKeyDown(tasterDesno)) PokusajKretanje(Vector2Int.right);
            else if (Input.GetKeyDown(tasterLevo)) PokusajKretanje(Vector2Int.left);
            else if (Input.GetKeyDown(tasterGore)) PokusajKretanje(Vector2Int.up);
            else if (Input.GetKeyDown(tasterDole)) PokusajKretanje(Vector2Int.down);
            return;
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
            PokusajKretanje(Vector2Int.right);
        else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
            PokusajKretanje(Vector2Int.left);
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            PokusajKretanje(Vector2Int.up);
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            PokusajKretanje(Vector2Int.down);
    }

    public void Pogini()
    {
        if (jeMrtav) return;
        jeMrtav = true;
        StopAllCoroutines();
        jeUKretanju = false;
        PrikaziVizuelnuSmrt();
        Debug.Log("Gari je poginuo!");
        OnSmrt?.Invoke();
    }

    void PrikaziVizuelnuSmrt()
    {
        foreach (Transform deo in deloviTela)
        {
            SpriteRenderer sr = deo.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = Color.red;
        }
    }

    void PokusajKretanje(Vector2Int noviSmer)
    {
        if (noviSmer == -trenutniSmer) return;

        Vector3 novaPozicijaGlave = deloviTela[0].position + new Vector3(noviSmer.x, noviSmer.y, 0);
        Physics2D.SyncTransforms();

        // 1. Provera Portala
        Portal portalNaPutu = NadjiKomponentuNaPoziciji<Portal>(novaPozicijaGlave);
        if (portalNaPutu != null)
        {
            if (portalNaPutu.tipPortala == Portal.TipPortala.Ulaz && portalNaPutu.povezaniPortal != null)
            {
                IzvrsiPrelazakKrozPortal(portalNaPutu);
                return;
            }
            else if (portalNaPutu.tipPortala == Portal.TipPortala.Izlaz)
            {
                return; // Izlazni portal deluje kao zid
            }
        }

        // 2. Fizičke blokade
        if (ImaObjekatSaTagom(novaPozicijaGlave, "Zemlja")) return;
        if (ImaTeloNaPoziciji(novaPozicijaGlave)) return;

        // 3. Provera za Kamen
        Kamen kamenNaPutu = NadjiKomponentuNaPoziciji<Kamen>(novaPozicijaGlave);
        if (kamenNaPutu != null)
        {
            bool uspesnoPomeren = kamenNaPutu.PokusajPomeranje(noviSmer);
            if (!uspesnoPomeren) return; // Ako kamen ne može da se pomeri, Gari ne može dalje
        }

        // 4. Provera neuspešnog skoka na kraj
        if (ImaObjekatSaTagom(novaPozicijaGlave, "Kraj") && noviSmer == Vector2Int.up)
        {
            List<Vector3> buducePozicije = GenerisiBuducePozicije(novaPozicijaGlave);
            if (!ImaOslonacZaPozicije(buducePozicije))
            {
                StartCoroutine(AnimacijaNeuspesnogSkoka(noviSmer));
                return;
            }
        }

        // 5. Provera za Vrata
        Vrata vrataNaPutu = NadjiKomponentuNaPoziciji<Vrata>(novaPozicijaGlave);
        if (vrataNaPutu != null && !vrataNaPutu.JeOtvoreno()) return;

        StartCoroutine(IzvrsiKretanje(noviSmer, novaPozicijaGlave));
    }

    void IzvrsiPrelazakKrozPortal(Portal ulazniPortal)
    {
        Portal izlazniPortal = ulazniPortal.povezaniPortal;
        Vector2Int smerIzlaza = izlazniPortal.DajVektorSmeraIzlaza();

        Vector3 novaPozicijaGlave = izlazniPortal.transform.position + new Vector3(smerIzlaza.x, smerIzlaza.y, 0);

        // Provera osnovnih prepreka na izlazu (Zemlja i Telo)
        if (ImaObjekatSaTagom(novaPozicijaGlave, "Zemlja") || ImaTeloNaPoziciji(novaPozicijaGlave)) return;

        // Provera za Vrata na izlazu
        Vrata vrataNaIzlazu = NadjiKomponentuNaPoziciji<Vrata>(novaPozicijaGlave);
        if (vrataNaIzlazu != null && !vrataNaIzlazu.JeOtvoreno()) return;

        // PROVERA ZA KAMEN NA IZLAZU PORTALA
        Kamen kamenNaIzlazu = NadjiKomponentuNaPoziciji<Kamen>(novaPozicijaGlave);
        if (kamenNaIzlazu != null)
        {
            bool uspesnoPomeren = kamenNaIzlazu.PokusajPomeranje(smerIzlaza);
            if (!uspesnoPomeren) return;
        }

        StartCoroutine(PraviRedosledTeleportacije(novaPozicijaGlave, smerIzlaza));
    }

    IEnumerator PraviRedosledTeleportacije(Vector3 novaPozicijaGlave, Vector2Int smerIzlaza)
    {
        jeUKretanju = true;

        List<Vector3> starePozicije = new List<Vector3>();
        foreach (Transform deo in deloviTela) starePozicije.Add(deo.position);

        deloviTela[0].position = novaPozicijaGlave;
        trenutniSmer = smerIzlaza;

        for (int i = 1; i < deloviTela.Count; i++)
            deloviTela[i].position = starePozicije[i - 1];

        Physics2D.SyncTransforms();
        yield return new WaitForSeconds(0.05f);

        ProveriSiljkeNaSvimDelovima();
        if (jeMrtav) yield break;

        ProveriJabuku(novaPozicijaGlave, starePozicije[starePozicije.Count - 1]);
        ProveriTruluJabuku(novaPozicijaGlave);
        bool stigaoNaKraj = ProveriKrajNivoa(novaPozicijaGlave);

        if (!stigaoNaKraj && !ImaOslonac())
        {
            yield return StartCoroutine(PadniAkoNemaOslonca());
        }

        if (!jeMrtav) jeUKretanju = false;
    }

    IEnumerator IzvrsiKretanje(Vector2Int noviSmer, Vector3 novaPozicijaGlave)
    {
        jeUKretanju = true;

        List<Vector3> pocetnePozicije = new List<Vector3>();
        List<Vector3> ciljnePozicije = new List<Vector3>();

        for (int i = 0; i < deloviTela.Count; i++)
        {
            pocetnePozicije.Add(deloviTela[i].position);
            if (i == 0)
            {
                ciljnePozicije.Add(novaPozicijaGlave);
                trenutniSmer = noviSmer;
            }
            else
            {
                ciljnePozicije.Add(pocetnePozicije[i - 1]);
            }
        }

        float protekloVreme = 0f;
        while (protekloVreme < trajanjeKoraka)
        {
            protekloVreme += Time.deltaTime;
            float t = protekloVreme / trajanjeKoraka;
            for (int i = 0; i < deloviTela.Count; i++)
            {
                if (Vector3.Distance(pocetnePozicije[i], ciljnePozicije[i]) > 1.5f)
                    deloviTela[i].position = ciljnePozicije[i];
                else
                    deloviTela[i].position = Vector3.Lerp(pocetnePozicije[i], ciljnePozicije[i], t);
            }
            yield return null;
        }

        for (int i = 0; i < deloviTela.Count; i++) deloviTela[i].position = ciljnePozicije[i];
        Physics2D.SyncTransforms();

        ProveriSiljkeNaSvimDelovima();
        if (jeMrtav) yield break;

        ProveriJabuku(ciljnePozicije[0], pocetnePozicije[pocetnePozicije.Count - 1]);
        ProveriTruluJabuku(novaPozicijaGlave);
        bool stigaoNaKraj = ProveriKrajNivoa(ciljnePozicije[0]);

        if (!stigaoNaKraj && !ImaOslonac())
        {
            yield return StartCoroutine(PadniAkoNemaOslonca());
        }

        if (!jeMrtav) jeUKretanju = false;
    }

    IEnumerator PadniAkoNemaOslonca()
    {
        while (!ImaOslonac())
        {
            if (jeMrtav) break;

            if (transform.position.y < granicaIspadanja)
            {
                Pogini();
                break;
            }

            List<Vector3> pocetnePozicije = new List<Vector3>();
            List<Vector3> ciljnePozicije = new List<Vector3>();

            foreach (Transform deo in deloviTela)
            {
                pocetnePozicije.Add(deo.position);
                ciljnePozicije.Add(deo.position + Vector3.down);
            }

            float protekloVreme = 0f;
            while (protekloVreme < trajanjeKoraka)
            {
                protekloVreme += Time.deltaTime;
                float t = protekloVreme / trajanjeKoraka;
                for (int i = 0; i < deloviTela.Count; i++)
                    deloviTela[i].position = Vector3.Lerp(pocetnePozicije[i], ciljnePozicije[i], t);
                yield return null;
            }

            for (int i = 0; i < deloviTela.Count; i++) deloviTela[i].position = ciljnePozicije[i];

            Physics2D.SyncTransforms();
            ProveriSiljkeNaSvimDelovima();
            if (jeMrtav) break;
        }
    }

    // ==========================================
    // REFAKTORISANE I UNIVERZALNE FUNKCIJE
    // ==========================================

    T NadjiKomponentuNaPoziciji<T>(Vector3 pozicija) where T : Component
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija, velicinaKutije, 0f);
        foreach (Collider2D hit in hitovi)
        {
            T komp = hit.GetComponent<T>();
            if (komp == null) komp = hit.GetComponentInParent<T>();
            if (komp != null) return komp;
        }
        return null;
    }

    Collider2D NadjiKolajderSaTagom(Vector3 pozicija, string tag)
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija, velicinaKutije, 0f);
        foreach (Collider2D hit in hitovi)
        {
            if (hit.CompareTag(tag)) return hit;
        }
        return null;
    }

    bool ImaObjekatSaTagom(Vector3 pozicija, string tag)
    {
        return NadjiKolajderSaTagom(pozicija, tag) != null;
    }

    bool JeOslonac(Collider2D hit)
    {
        // TRULA JABUKA JE DODATA KAO OSLONAC OVDE
        return hit.CompareTag("Zemlja") || hit.CompareTag("Kraj") || hit.CompareTag("Jabuka") ||
               hit.CompareTag("TrulaJabuka") || hit.CompareTag("Kamen") || hit.CompareTag("Portal");
    }

    bool ImaOslonacNaPoziciji(Vector3 pozicija)
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija + Vector3.down, velicinaKutije, 0f);
        foreach (Collider2D hit in hitovi)
        {
            if (JeOslonac(hit)) return true;
        }
        return false;
    }

    bool ImaOslonac()
    {
        Physics2D.SyncTransforms();
        foreach (Transform deo in deloviTela)
        {
            if (ImaOslonacNaPoziciji(deo.position)) return true;
        }
        return false;
    }

    bool ImaOslonacZaPozicije(List<Vector3> pozicije)
    {
        Physics2D.SyncTransforms();
        foreach (Vector3 poz in pozicije)
        {
            if (ImaOslonacNaPoziciji(poz)) return true;
        }
        return false;
    }

    // ==========================================
    // OSTALE FUNKCIJE
    // ==========================================

    List<Vector3> GenerisiBuducePozicije(Vector3 novaPozicijaGlave)
    {
        List<Vector3> buduce = new List<Vector3>();
        buduce.Add(novaPozicijaGlave);
        for (int i = 1; i < deloviTela.Count; i++) buduce.Add(deloviTela[i - 1].position);
        return buduce;
    }

    IEnumerator AnimacijaNeuspesnogSkoka(Vector2Int smer)
    {
        jeUKretanju = true;

        List<Vector3> pocetne = new List<Vector3>();
        List<Vector3> pomaknute = new List<Vector3>();
        Vector3 pomak = new Vector3(smer.x, smer.y, 0) * 0.25f;

        foreach (Transform deo in deloviTela)
        {
            pocetne.Add(deo.position);
            pomaknute.Add(deo.position + pomak);
        }

        float polaTrajanja = trajanjeKoraka * 0.5f;

        float proteklo = 0f;
        while (proteklo < polaTrajanja)
        {
            proteklo += Time.deltaTime;
            float t = proteklo / polaTrajanja;
            for (int i = 0; i < deloviTela.Count; i++)
                deloviTela[i].position = Vector3.Lerp(pocetne[i], pomaknute[i], t);
            yield return null;
        }

        proteklo = 0f;
        while (proteklo < polaTrajanja)
        {
            proteklo += Time.deltaTime;
            float t = proteklo / polaTrajanja;
            for (int i = 0; i < deloviTela.Count; i++)
                deloviTela[i].position = Vector3.Lerp(pomaknute[i], pocetne[i], t);
            yield return null;
        }

        for (int i = 0; i < deloviTela.Count; i++) deloviTela[i].position = pocetne[i];
        jeUKretanju = false;
    }

    bool ImaTeloNaPoziciji(Vector3 pozicija)
    {
        for (int i = 1; i < deloviTela.Count; i++)
        {
            if (Vector3.Distance(deloviTela[i].position, pozicija) < 0.2f) return true;
        }
        return false;
    }

    void ProveriSiljkeNaSvimDelovima()
    {
        foreach (Transform deo in deloviTela)
        {
            if (ImaObjekatSaTagom(deo.position, "Siljak"))
            {
                Pogini();
                return;
            }
        }
    }

    void ProveriJabuku(Vector3 pozicija, Vector3 zadnjaStaraPozicija)
    {
        Collider2D jabukaHit = NadjiKolajderSaTagom(pozicija, "Jabuka");
        if (jabukaHit != null)
        {
            Destroy(jabukaHit.gameObject);
            GameObject novoTelo = Instantiate(teloPrefab, zadnjaStaraPozicija, Quaternion.identity, transform);
            deloviTela.Add(novoTelo.transform);
        }
    }

    bool ProveriKrajNivoa(Vector3 pozicija)
    {
        if (ImaObjekatSaTagom(pozicija, "Kraj"))
        {
            if (jeMultiplayer && mecMenadzer != null)
            {
                mecMenadzer.IgracJeZavrsio(indeksIgraca);
            }
            else
            {
                NivoUIManager manager = FindObjectOfType<NivoUIManager>();
                if (manager != null) manager.PrikaziPobedu();
            }
            return true;
        }
        return false;
    }

    void PostaviPocetniSmer()
    {
        switch (pocetniSmerGledanja)
        {
            case SmerKretanja.Levo: trenutniSmer = Vector2Int.left; break;
            case SmerKretanja.Desno: trenutniSmer = Vector2Int.right; break;
            case SmerKretanja.Gore: trenutniSmer = Vector2Int.up; break;
            case SmerKretanja.Dole: trenutniSmer = Vector2Int.down; break;
        }
    }

    void KreirayPocetnoTelo()
    {
        Vector3 trenutnaPozicija = transform.position;
        Vector3 smerTela = new Vector3(-trenutniSmer.x, -trenutniSmer.y, 0);

        for (int i = 0; i < pocetnaDuzinaTela; i++)
        {
            trenutnaPozicija += smerTela;
            GameObject novoTelo = Instantiate(teloPrefab, trenutnaPozicija, Quaternion.identity, transform);
            deloviTela.Add(novoTelo.transform);
        }
    }

    IEnumerator ProveriPocetnuGravitaciju()
    {
        jeUKretanju = true;
        yield return new WaitForFixedUpdate();
        Physics2D.SyncTransforms();

        if (!ImaOslonac())
        {
            yield return StartCoroutine(PadniAkoNemaOslonca());
        }

        if (!jeMrtav) jeUKretanju = false;
    }

    public void SmanjiTelo()
    {
        if (deloviTela.Count <= 2)
        {
            Debug.Log("Gari je pojeo trulu jabuku ali je bio prekratak da preživi!");
            Pogini();
            return;
        }

        int poslednjiIndeks = deloviTela.Count - 1;
        Transform repZaUklanjanje = deloviTela[poslednjiIndeks];
        deloviTela.RemoveAt(poslednjiIndeks);
        Destroy(repZaUklanjanje.gameObject);

        Debug.Log($"Gari je pojeo trulu jabuku i smanjio se! Nova dužina: {deloviTela.Count}");
    }

    void ProveriTruluJabuku(Vector3 pozicija)
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija, velicinaKutije, 0f);
        foreach (Collider2D hit in hitovi)
        {
            if (hit.CompareTag("TrulaJabuka") || hit.GetComponent<TrulaJabuka>() != null)
            {
                Destroy(hit.gameObject);
                SmanjiTelo();
                break;
            }
        }
    }
}