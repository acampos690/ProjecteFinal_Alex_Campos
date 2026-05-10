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
        //sitodavía no hay ningún UIManager global
        if (Instance == null)
        {
            //guardamos este objeto como el único Instance
            Instance = this;
        }
        else
        {
            //si ya existe otro UIManager, destruimos este para evitar duplicados
            Destroy(gameObject);
        }
    }

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

    public void ActualizarTextoMonedas(int monedas)
    {
        if (textoMonedas != null)
        {
            // Concatenamos la 'x' con el número
            textoMonedas.text = "x" + monedas.ToString();
        }
    }
}
