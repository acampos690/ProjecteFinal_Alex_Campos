using UnityEngine;

public class PausarJuego : MonoBehaviour
{
   public GameObject menuPausa; // Asigna el menú de pausa en el inspector
   public bool juegoPausado = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    public void Reanudar()
    {
        menuPausa.SetActive(false); // Oculta el menú de pausa
        Time.timeScale = 1; // Reanuda el tiempo del juego
        juegoPausado = false;
    }

    public void Pausar()
    {
        menuPausa.SetActive(true); // Muestra el menú de pausa
        Time.timeScale = 0; // Detiene el tiempo del juego
        juegoPausado = true;
    }   
}


