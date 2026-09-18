using UnityEngine;

public class Siljak : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Proveravamo da li je objekat koji je dotakao siljak glava ("Player") ili deo tela ("Telo")
        if (collision.CompareTag("Player") || collision.CompareTag("Telo"))
        {
            CrvKretanje crv = collision.GetComponentInParent<CrvKretanje>();

            if (crv == null)
            {
                crv = FindObjectOfType<CrvKretanje>();
            }

            if (crv != null)
            {
                crv.Pogini();
            }
        }
    }
}