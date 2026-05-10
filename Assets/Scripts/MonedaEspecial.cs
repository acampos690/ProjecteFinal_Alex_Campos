using UnityEngine;

public class MonedaEspecial : MonoBehaviour
{
    public float multiplicador = 2f;
    public float duracion = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.ActivarBoost(multiplicador, duracion);
            }

            Destroy(gameObject);
        }
    }
}
