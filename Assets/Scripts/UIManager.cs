using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Vida")]
    public Image[] corazones;
    public Sprite corazonLleno;
    public Sprite corazonMedio;
    public Sprite corazonVacio;

    [Header("Pausa")]
    public GameObject menuPausa;

    public static UIManager Instance;

    public TextMeshProUGUI textoMonedas;

    void Awake()
    {
        // Singleton para que solo exista un UIManager
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // Actualiza la UI de vida con corazones
    public void ActualizarCorazones(int vidaActual, int vidaMaxima)
    {
        int vidaTemp = vidaActual;

        for (int i = 0; i < corazones.Length; i++)
        {
            if (vidaTemp >= 2)
            {
                corazones[i].sprite = corazonLleno;
                vidaTemp -= 2;
            }
            else if (vidaTemp == 1)
            {
                corazones[i].sprite = corazonMedio;
                vidaTemp -= 1;
            }
            else
            {
                corazones[i].sprite = corazonVacio;
            }
        }
    }

    // Muestra u oculta el menú de pausa
    public void MostrarMenuPausa(bool mostrar)
    {
        if (menuPausa != null)
            menuPausa.SetActive(mostrar);
    }

    // Actualiza el contador de monedas en pantalla
    public void ActualizarTextoMonedas(int monedas)
    {
        if (textoMonedas != null)
        {
            textoMonedas.text = "x" + monedas;
        }
    }
}