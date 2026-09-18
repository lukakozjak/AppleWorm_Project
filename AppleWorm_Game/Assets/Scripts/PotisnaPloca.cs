using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotisnaPloca : MonoBehaviour
{
    [Header("Povezana Vrata")]
    public Vrata ciljnaVrata; // Prevuci vrata iz Hijerarhije ovde

    [Header("Vizuelni Delovi Ploče")]
    public Transform gornjiPokretniDeo;
    public float dubinaPritiska = 0.15f; // Koliko se gornji deo spusti dole
    public float brzinaPritiska = 8f;

    private Vector3 pocetnaLokalnaPozicija;
    private Vector3 pritisnutaLokalnaPozicija;
    private Vector2 velicinaKutije = new Vector2(0.7f, 0.7f);
    private bool jePritisnuta = false;

    void Start()
    {
        if (gornjiPokretniDeo != null)
        {
            pocetnaLokalnaPozicija = gornjiPokretniDeo.localPosition;
            pritisnutaLokalnaPozicija = pocetnaLokalnaPozicija - new Vector3(0, dubinaPritiska, 0);
        }
    }

    void Update()
    {
        ProveriPritisak();
    }

    void ProveriPritisak()
    {
        Physics2D.SyncTransforms();

        Collider2D[] hitovi = Physics2D.OverlapBoxAll(transform.position, velicinaKutije, 0f);
        bool imaNekoga = false;

        foreach (Collider2D hit in hitovi)
        {
            // Proverava da li na ploči stoji Gari (glava ili telo) ili Kamen
            if (hit.CompareTag("Player") || hit.CompareTag("Telo") || hit.CompareTag("Kamen"))
            {
                imaNekoga = true;
                break;
            }
        }

        if (imaNekoga && !jePritisnuta)
        {
            jePritisnuta = true;
            if (ciljnaVrata != null) ciljnaVrata.Otvori();
        }
        else if (!imaNekoga && jePritisnuta)
        {
            jePritisnuta = false;
            if (ciljnaVrata != null) ciljnaVrata.Zatvori();
        }

        // Pomera gornji deo ploče glatko gore/dole
        if (gornjiPokretniDeo != null)
        {
            Vector3 cilj = jePritisnuta ? pritisnutaLokalnaPozicija : pocetnaLokalnaPozicija;
            gornjiPokretniDeo.localPosition = Vector3.Lerp(gornjiPokretniDeo.localPosition, cilj, Time.deltaTime * brzinaPritiska);
        }
    }
}