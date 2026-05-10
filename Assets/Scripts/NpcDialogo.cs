using UnityEngine;
using TMPro;

public class NpcDialogo : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelDialogo;
    public TextMeshProUGUI textoDialogo;
    public GameObject textoInteractuar;

    [TextArea(3, 5)]
    public string[] dialogos;

    private int indice = 0;
    private bool jugadorCerca = false;
    private bool dialogoActivo = false;

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (!dialogoActivo)
            {
                IniciarDialogo();
            }
            else
            {
                SiguienteDialogo();
            }
        }
    }

    void IniciarDialogo()
    {
        dialogoActivo = true;

        panelDialogo.SetActive(true);
        textoInteractuar.SetActive(false);


        textoDialogo.text = dialogos[indice];
    }

    void SiguienteDialogo()
    {
        indice++;

        if (indice < dialogos.Length)
        {
            textoDialogo.text = dialogos[indice];
        }
        else
        {
            CerrarDialogo();
        }
    }

    void CerrarDialogo()
    {
        dialogoActivo = false;
        panelDialogo.SetActive(false);
        textoInteractuar.SetActive(true);
        indice = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
        }   
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            CerrarDialogo();
        }
    }
}