using UnityEngine;
using UnityEngine.UI;

public class ZonaCueva : MonoBehaviour
{
    public Image oscuridad; // arrastra aquí la imagen negra
    public float alphaOscuro = 0.8f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("ENTRÓ EN LA CUEVA");
            Debug.Log("Oscuridad es: " + oscuridad);

            Color c = oscuridad.color;
            c.a = alphaOscuro;
            oscuridad.color = c;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Color c = oscuridad.color;
            c.a = 0f;
            oscuridad.color = c;
        }
    }
}