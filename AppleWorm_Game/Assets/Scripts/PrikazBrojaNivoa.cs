using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

// Automatski prikazuje broj nivoa (npr "01", "15", "30") citajuci ime scene.
// Ne treba nikakvo rucno podesavanje po nivou - radi u svih 30 scena isto.
public class PrikazBrojaNivoa : MonoBehaviour
{
    public TMP_Text tekstBroja;

    void Start()
    {
        if (tekstBroja == null) return;

        string imeScene = SceneManager.GetActiveScene().name; // ocekuje "Nivo_X"
        string[] delovi = imeScene.Split('_');

        if (delovi.Length == 2 && int.TryParse(delovi[1], out int broj))
        {
            // Dopunjava nulom do dve cifre: 1 -> "01", 15 -> "15", 104 -> "104"
            tekstBroja.text = broj.ToString("00");
            tekstBroja.gameObject.SetActive(true);
        }
        else
        {
            // Nismo u nivou (npr. Arena1v1 ili meni) - sakrij brojac
            tekstBroja.gameObject.SetActive(false);
        }
    }
}