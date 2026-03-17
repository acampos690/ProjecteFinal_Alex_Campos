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

        Debug.Log("UI actualiza corazones. Vida: " + vidaActual);

    }

    public void MostrarMenuPausa(bool mostrar)
    {
        if (menuPausa != null)
            menuPausa.SetActive(mostrar);
    }
}
