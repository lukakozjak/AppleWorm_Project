using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    // Funkcija koja vraca igraca u Glavni Meni
    public void VratiUMeni()
    {
        // Ucitava scenu pod nazivom "GlavniMeni"
        SceneManager.LoadScene("GlavniMeni");
    }
}