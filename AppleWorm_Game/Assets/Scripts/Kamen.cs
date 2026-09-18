using System.Collections;
using UnityEngine;

/// <summary>
/// Upravlja logikom kretanja, guranja, padanja i teleportacije kamena u igri.
/// </summary>
public class Kamen : MonoBehaviour
{
    [Header("Podešavanja Animacije i Fizike")]
    public float brzinaPadanja = 8f;        // Brzina kojom kamen klizi nadole pri padu
    public float trajanjeGuranja = 0.12f;   // Vreme trajanja animacije guranja (usklađeno sa Garijem)
    public Vector2 velicinaKutije = new Vector2(0.7f, 0.7f); // Osnovna veličina kutije za proveru

    private bool jeUKretanju = false;       // Označava da li se kamen trenutno pomera, teleportuje ili pada

    private void Update()
    {
        // Ako kamen ne vrši kretanje, proveravamo da li je stao na ulazni portal ili je ostao bez oslonca
        if (!jeUKretanju)
        {
            Portal portalNaMestu = NadjiPortalNaPoziciji(transform.position);
            if (portalNaMestu != null && portalNaMestu.tipPortala == Portal.TipPortala.Ulaz && portalNaMestu.povezaniPortal != null)
            {
                ProveriIProddjiKrozPortal(portalNaMestu);
                return;
            }

            if (!ImaOslonacIspod())
            {
                StartCoroutine(PadniGlatko());
            }
        }
    }

    /// <summary>
    /// Poziva se iz CrvKretanje.cs kada Gari pogura kamen u bilo kom smeru.
    /// </summary>
    public bool PokusajPomeranje(Vector2Int smer)
    {
        if (jeUKretanju) return false;

        Vector3 novaPozicija = transform.position + new Vector3(smer.x, smer.y, 0);
        Physics2D.SyncTransforms();

        // 1. Provera da li na novoj poziciji postoji Portal
        Portal portalNaPutu = NadjiPortalNaPoziciji(novaPozicija);
        if (portalNaPutu != null)
        {
            // Izlazni portal deluje kao zid
            if (portalNaPutu.tipPortala == Portal.TipPortala.Izlaz)
            {
                return false;
            }

            // Ulazni portal teleportuje kamen na drugu stranu
            if (portalNaPutu.tipPortala == Portal.TipPortala.Ulaz && portalNaPutu.povezaniPortal != null)
            {
                return ProveriIProddjiKrozPortal(portalNaPutu);
            }
        }

        // 2. Ako nema portala, proveravamo standardne prepreke
        if (ImaPreprekuNaPoziciji(novaPozicija))
        {
            return false; // Prolaz je blokiran
        }

        // 3. Pokrećemo animaciju guranja
        StartCoroutine(AnimirajGuranje(novaPozicija, smer));
        return true;
    }

    /// <summary>
    /// Teleportuje kamen na izlaznu stranu povezanog portala ako je mesto slobodno.
    /// </summary>
    private bool ProveriIProddjiKrozPortal(Portal ulazniPortal)
    {
        Portal izlazniPortal = ulazniPortal.povezaniPortal;
        Vector2Int smerIzlaza = izlazniPortal.DajVektorSmeraIzlaza();
        Vector3 pozicijaIzlaza = izlazniPortal.transform.position + new Vector3(smerIzlaza.x, smerIzlaza.y, 0);

        // Proveravamo da li je mesto na izlazu slobodno
        if (ImaPreprekuNaPoziciji(pozicijaIzlaza))
        {
            return false; // Izlaz je zauzet
        }

        StartCoroutine(TeleportujKamenKrozPortal(pozicijaIzlaza));
        return true;
    }

    /// <summary>
    /// Izvršava teleportaciju kamena na izlaz portala i proverava gravitaciju.
    /// </summary>
    private IEnumerator TeleportujKamenKrozPortal(Vector3 ciljnaPozicija)
    {
        jeUKretanju = true;

        transform.position = ciljnaPozicija;
        Physics2D.SyncTransforms();

        yield return new WaitForSeconds(0.05f);

        if (!ImaOslonacIspod())
        {
            yield return StartCoroutine(PadniGlatko());
        }
        else
        {
            jeUKretanju = false;
        }
    }

    /// <summary>
    /// Glatko pomera kamen na novo polje u trajanju Garijeve animacije koraka.
    /// </summary>
    private IEnumerator AnimirajGuranje(Vector3 ciljnaPozicija, Vector2Int smer)
    {
        jeUKretanju = true;

        Vector3 pocetnaPozicija = transform.position;
        float protekloVreme = 0f;

        while (protekloVreme < trajanjeGuranja)
        {
            protekloVreme += Time.deltaTime;
            float t = protekloVreme / trajanjeGuranja;
            transform.position = Vector3.Lerp(pocetnaPozicija, ciljnaPozicija, t);
            yield return null;
        }

        transform.position = ciljnaPozicija;
        Physics2D.SyncTransforms();

        if (smer != Vector2Int.up && !ImaOslonacIspod())
        {
            yield return StartCoroutine(PadniGlatko());
        }
        else
        {
            jeUKretanju = false;
        }
    }

    /// <summary>
    /// Proverava da li na datoj poziciji postoji bilo kakva prepreka.
    /// </summary>
    public bool ImaPreprekuNaPoziciji(Vector3 pozicija)
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija, velicinaKutije, 0f);

        foreach (Collider2D hit in hitovi)
        {
            if (hit.CompareTag("Zemlja") ||
                hit.CompareTag("Kamen") ||
                hit.CompareTag("Player") ||
                hit.CompareTag("Telo") ||
                hit.CompareTag("Portal") ||
                hit.CompareTag("Kraj") ||
                hit.CompareTag("Jabuka") ||
                hit.CompareTag("TrulaJabuka"))
            {
                return true;
            }

            Vrata vrata = hit.GetComponent<Vrata>();
            if (vrata != null && !vrata.JeOtvoreno())
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Proverava da li kamen ima podlogu direktno ispod sebe.
    /// </summary>
    public bool ImaOslonacIspod()
    {
        Vector3 pozicijaIspod = transform.position + Vector3.down;
        Physics2D.SyncTransforms();

        // Proвера свих објеката који могу да послуже као ослонац испод камена
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicijaIspod, new Vector2(0.95f, 0.95f), 0f);

        foreach (Collider2D hit in hitovi)
        {
            if (hit.CompareTag("Zemlja") ||
                hit.CompareTag("Kamen") ||
                hit.CompareTag("Player") ||
                hit.CompareTag("Telo") ||
                hit.CompareTag("Kraj") ||
                hit.CompareTag("Portal") ||
                hit.CompareTag("Jabuka") ||
                hit.CompareTag("TrulaJabuka"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Pronalazi komponentu Portal na zadatoj poziciji.
    /// </summary>
    private Portal NadjiPortalNaPoziciji(Vector3 pozicija)
    {
        Collider2D[] hitovi = Physics2D.OverlapBoxAll(pozicija, velicinaKutije, 0f);
        foreach (Collider2D hit in hitovi)
        {
            Portal p = hit.GetComponentInParent<Portal>();
            if (p != null) return p;
        }
        return null;
    }

    /// <summary>
    /// Animacija postepenog padanja kamena za jedno polje nadole.
    /// </summary>
    private IEnumerator PadniGlatko()
    {
        jeUKretanju = true;

        Vector3 pocetnaPozicija = transform.position;
        Vector3 ciljnaPozicija = transform.position + Vector3.down;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * brzinaPadanja;
            transform.position = Vector3.Lerp(pocetnaPozicija, ciljnaPozicija, t);
            yield return null;
        }

        transform.position = ciljnaPozicija;
        Physics2D.SyncTransforms();

        if (!ImaOslonacIspod())
        {
            yield return StartCoroutine(PadniGlatko());
        }
        else
        {
            jeUKretanju = false;
        }
    }
}