using UnityEngine;

// Resava upozorenje "There can be only one active Event System".
// Prvi EventSystem koji se pokrene postaje trajan i prezivljava promene scena;
// svaki sledeci koji naidje - unisti sam sebe.
// Zakaci ovu skriptu na EventSystem objekat u SVAKOJ sceni.
public class TrajniEventSystem : MonoBehaviour
{
    private static TrajniEventSystem instanca;

    void Awake()
    {
        if (instanca != null && instanca != this)
        {
            Destroy(gameObject);
            return;
        }

        instanca = this;
        DontDestroyOnLoad(gameObject);
    }
}