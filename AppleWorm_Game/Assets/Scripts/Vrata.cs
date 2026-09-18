using System.Collections;
using UnityEngine;

public class Vrata : MonoBehaviour
{
    [Header("Podešavanja Animacije")]
    public Transform pokretniDeoVrata; // Unutrašnji deo vrata koji se skuplja/pomaže
    public float brzinaOtvaranja = 5f;

    [Header("Pozicije (Lokalne)")]
    public Vector3 lokalnaPozicijaZatvoreno = Vector3.zero;
    public Vector3 lokalnaPozicijaOtvoreno = new Vector3(0, 0.8f, 0); // Pomera se ka jednom od okvira
    public Vector3 skalazaZatvoreno = Vector3.one;
    public Vector3 skalazaOtvoreno = new Vector3(1f, 0.1f, 1f); // Skuplja se da izgleda uvučeno

    private BoxCollider2D validatorKolidora;
    private bool jeOtvoreno = false;
    private Coroutine trenutnaAnimacija;

    void Awake()
    {
        validatorKolidora = GetComponent<BoxCollider2D>();
        if (pokretniDeoVrata == null)
        {
            pokretniDeoVrata = transform;
        }
    }

    public void Otvori()
    {
        if (jeOtvoreno) return;
        jeOtvoreno = true;

        if (validatorKolidora != null)
            validatorKolidora.enabled = false; // Isključuje fiziku da Gari može da prođe

        if (trenutnaAnimacija != null) StopCoroutine(trenutnaAnimacija);
        trenutnaAnimacija = StartCoroutine(AnimirajVrata(lokalnaPozicijaOtvoreno, skalazaOtvoreno));
    }

    public void Zatvori()
    {
        if (!jeOtvoreno) return;
        jeOtvoreno = false;

        if (validatorKolidora != null)
            validatorKolidora.enabled = true; // Ponovo aktivira fiziku (blokadu)

        if (trenutnaAnimacija != null) StopCoroutine(trenutnaAnimacija);
        trenutnaAnimacija = StartCoroutine(AnimirajVrata(lokalnaPozicijaZatvoreno, skalazaZatvoreno));
    }

    IEnumerator AnimirajVrata(Vector3 ciljnaPozicija, Vector3 ciljnaSkalaza)
    {
        while (Vector3.Distance(pokretniDeoVrata.localPosition, ciljnaPozicija) > 0.01f ||
               Vector3.Distance(pokretniDeoVrata.localScale, ciljnaSkalaza) > 0.01f)
        {
            pokretniDeoVrata.localPosition = Vector3.Lerp(pokretniDeoVrata.localPosition, ciljnaPozicija, Time.deltaTime * brzinaOtvaranja);
            pokretniDeoVrata.localScale = Vector3.Lerp(pokretniDeoVrata.localScale, ciljnaSkalaza, Time.deltaTime * brzinaOtvaranja);
            yield return null;
        }

        pokretniDeoVrata.localPosition = ciljnaPozicija;
        pokretniDeoVrata.localScale = ciljnaSkalaza;
    }

    public bool JeOtvoreno()
    {
        return jeOtvoreno;
    }
}