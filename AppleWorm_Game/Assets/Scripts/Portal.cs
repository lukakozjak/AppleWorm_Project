using UnityEngine;

public class Portal : MonoBehaviour
{
    public enum TipPortala { Ulaz, Izlaz, Dvosmerni }
    public enum Smer { Gore, Dole, Levo, Desno } // Novi padajući meni za smer

    [Header("Osnovne Postavke")]
    public TipPortala tipPortala;
    public Portal povezaniPortal;

    [Header("Smer Izlaza")]
    [Tooltip("Izaberi u kom smeru objekat izlazi iz ovog portala")]
    public Smer smerIzlaza = Smer.Desno;

    // Prevodi tvoj izbor iz menija u koordinate koje razume CrvKretanje
    public Vector2Int DajVektorSmeraIzlaza()
    {
        switch (smerIzlaza)
        {
            case Smer.Gore: return Vector2Int.up;
            case Smer.Dole: return Vector2Int.down;
            case Smer.Levo: return Vector2Int.left;
            case Smer.Desno: return Vector2Int.right;
            default: return Vector2Int.right;
        }
    }

}